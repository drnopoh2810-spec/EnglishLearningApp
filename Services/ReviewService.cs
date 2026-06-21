using EnglishLearningApp.Data;
using EnglishLearningApp.Models;
using EnglishLearningApp.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningApp.Services
{
    public class ReviewService
    {
        private readonly AppDbContext _context;
        private readonly SentenceRepository _sentenceRepository;
        private readonly ReviewRepository _reviewRepository;

        private static readonly Random _random = new();

        // Spaced repetition intervals (in days)
        private readonly Dictionary<ReviewRating, double> _baseIntervals = new()
        {
            { ReviewRating.Again, 1 },
            { ReviewRating.Hard, 3 },
            { ReviewRating.Good, 7 },
            { ReviewRating.Easy, 14 }
        };

        // Ease factors for each rating
        private readonly Dictionary<ReviewRating, double> _easeFactors = new()
        {
            { ReviewRating.Again, 1.3 },
            { ReviewRating.Hard, 1.7 },
            { ReviewRating.Good, 2.0 },
            { ReviewRating.Easy, 2.5 }
        };

        public ReviewService(AppDbContext context, SentenceRepository sentenceRepository, ReviewRepository reviewRepository)
        {
            _context = context;
            _sentenceRepository = sentenceRepository;
            _reviewRepository = reviewRepository;
        }

        /// <summary>
        /// Processes a review rating and updates the sentence's schedule
        /// </summary>
        public async Task<ReviewResult> ProcessReviewAsync(int sentenceId, ReviewRating rating)
        {
            var sentence = await _sentenceRepository.GetByIdAsync(sentenceId);
            if (sentence == null)
                throw new ArgumentException("Sentence not found", nameof(sentenceId));

            var previousMastery = sentence.MasteryScore;

            var now = DateTime.UtcNow;
            var lastReview = await _reviewRepository.GetLatestReviewAsync(sentenceId);
            var intervalDays = CalculateInterval(sentence, rating, lastReview);
            var nextReview = now.AddDays(intervalDays);
            var newMasteryScore = CalculateMasteryScore(sentence, rating);

            // Create review record
            var review = new Review
            {
                SentenceId = sentenceId,
                ReviewDate = now,
                Rating = rating,
                NextReviewDate = nextReview
            };

            // Update sentence
            sentence.LastReviewDate = now;
            sentence.NextReviewDate = nextReview;
            sentence.MasteryScore = Math.Min(100, newMasteryScore);
            sentence.ReviewCount++;

            await _reviewRepository.AddAsync(review);
            await _sentenceRepository.UpdateAsync(sentence);

            return new ReviewResult
            {
                PreviousMastery = previousMastery,
                NewMastery = sentence.MasteryScore,
                NextReviewDate = nextReview,
                IntervalDays = intervalDays,
                ReviewCount = sentence.ReviewCount
            };
        }

        /// <summary>
        /// Gets prioritized sentences for review (overdue -> low mastery -> new)
        /// </summary>
        public async Task<IEnumerable<Sentence>> GetPrioritizedSentencesAsync(int maxCount = 20)
        {
            var today = DateTime.UtcNow.Date;

            // Get overdue sentences (priority 1)
            var overdue = await _context.Sentences
                .Include(s => s.GroupLinks)
                .ThenInclude(gl => gl.Group)
                .Where(s => s.NextReviewDate.HasValue && s.NextReviewDate.Value.Date < today)
                .OrderBy(s => s.NextReviewDate)
                .ThenBy(s => s.MasteryScore)
                .Take(maxCount)
                .ToListAsync();

            if (overdue.Count >= maxCount)
                return overdue;

            // Get due today sentences (priority 2)
            var dueToday = await _context.Sentences
                .Include(s => s.GroupLinks)
                .ThenInclude(gl => gl.Group)
                .Where(s => s.NextReviewDate.HasValue && s.NextReviewDate.Value.Date == today)
                .OrderBy(s => s.MasteryScore)
                .Take(maxCount - overdue.Count)
                .ToListAsync();

            var result = overdue.Concat(dueToday).ToList();
            if (result.Count >= maxCount)
                return result;

            // Get new sentences (never reviewed) (priority 3)
            var newSentences = await _context.Sentences
                .Include(s => s.GroupLinks)
                .ThenInclude(gl => gl.Group)
                .Where(s => s.ReviewCount == 0)
                .OrderByDescending(s => s.CreatedDate)
                .Take(maxCount - result.Count)
                .ToListAsync();

            result = result.Concat(newSentences).ToList();
            if (result.Count >= maxCount)
                return result;

            // Get low mastery sentences not yet due (priority 4)
            var lowMastery = await _context.Sentences
                .Include(s => s.GroupLinks)
                .ThenInclude(gl => gl.Group)
                .Where(s => s.MasteryScore < 50 && s.NextReviewDate > today)
                .OrderBy(s => s.MasteryScore)
                .ThenBy(s => s.NextReviewDate)
                .Take(maxCount - result.Count)
                .ToListAsync();

            result = result.Concat(lowMastery).ToList();

            return result.DistinctBy(s => s.Id).ToList();
        }

        /// <summary>
        /// Resets a sentence's review history
        /// </summary>
        public async Task ResetSentenceAsync(int sentenceId)
        {
            var sentence = await _sentenceRepository.GetByIdAsync(sentenceId);
            if (sentence == null) return;

            // Delete all reviews
            await _reviewRepository.DeleteAllReviewsForSentenceAsync(sentenceId);

            // Reset sentence stats
            sentence.MasteryScore = 0;
            sentence.ReviewCount = 0;
            sentence.LastReviewDate = null;
            sentence.NextReviewDate = null;

            await _sentenceRepository.UpdateAsync(sentence);
        }

        /// <summary>
        /// Calculates the review interval based on rating and history
        /// </summary>
        private double CalculateInterval(Sentence sentence, ReviewRating rating, Review? lastReview)
        {
            if (rating == ReviewRating.Again)
            {
                return _baseIntervals[ReviewRating.Again];
            }

            double baseInterval;
            if (lastReview == null || sentence.ReviewCount <= 1)
            {
                baseInterval = _baseIntervals[rating];
            }
            else
            {
                var previousInterval = (lastReview.NextReviewDate - lastReview.ReviewDate).TotalDays;
                var easeFactor = _easeFactors[rating];
                baseInterval = previousInterval * easeFactor;
                baseInterval = Math.Min(baseInterval, 180);
            }

            // Add small random factor to prevent cards from clustering
            var fuzz = _random.NextDouble() * 0.1 - 0.05; // +/- 5%
            baseInterval *= (1 + fuzz);

            return Math.Max(1, baseInterval);
        }

        /// <summary>
        /// Calculates new mastery score based on rating
        /// </summary>
        private double CalculateMasteryScore(Sentence sentence, ReviewRating rating)
        {
            var currentScore = sentence.MasteryScore;
            var reviewCount = sentence.ReviewCount;

            return rating switch
            {
                ReviewRating.Again => Math.Max(0, currentScore - 15),
                ReviewRating.Hard => currentScore + 5,
                ReviewRating.Good => currentScore + 10,
                ReviewRating.Easy => currentScore + 15,
                _ => currentScore
            };
        }

        /// <summary>
        /// Gets the next review date for a sentence
        /// </summary>
        public async Task<DateTime?> GetNextReviewDateAsync(int sentenceId)
        {
            var sentence = await _sentenceRepository.GetByIdAsync(sentenceId);
            return sentence?.NextReviewDate;
        }
    }

    public class ReviewResult
    {
        public double PreviousMastery { get; set; }
        public double NewMastery { get; set; }
        public DateTime NextReviewDate { get; set; }
        public double IntervalDays { get; set; }
        public int ReviewCount { get; set; }
    }
}
