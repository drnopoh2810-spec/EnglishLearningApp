-- Initial Migration SQL for English Learning App
-- Run this if EF Core migrations fail

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" TEXT NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
    "ProductVersion" TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS "Users" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Users" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "CreatedDate" TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS "SentenceGroups" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SentenceGroups" PRIMARY KEY AUTOINCREMENT,
    "GroupName" TEXT NOT NULL,
    "Description" TEXT,
    "CreatedDate" TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS "Sentences" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Sentences" PRIMARY KEY AUTOINCREMENT,
    "EnglishSentence" TEXT NOT NULL,
    "ArabicTranslation" TEXT,
    "DifficultyLevel" INTEGER NOT NULL,
    "MasteryScore" REAL NOT NULL,
    "ReviewCount" INTEGER NOT NULL,
    "LastReviewDate" TEXT,
    "NextReviewDate" TEXT,
    "CreatedDate" TEXT NOT NULL,
    "Notes" TEXT,
    "YouGlishUrl" TEXT
);

CREATE TABLE IF NOT EXISTS "SentenceGroupLinks" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_SentenceGroupLinks" PRIMARY KEY AUTOINCREMENT,
    "SentenceId" INTEGER NOT NULL,
    "GroupId" INTEGER NOT NULL,
    CONSTRAINT "FK_SentenceGroupLinks_Sentences_SentenceId" FOREIGN KEY ("SentenceId") REFERENCES "Sentences" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_SentenceGroupLinks_SentenceGroups_GroupId" FOREIGN KEY ("GroupId") REFERENCES "SentenceGroups" ("Id") ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS "Reviews" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Reviews" PRIMARY KEY AUTOINCREMENT,
    "SentenceId" INTEGER NOT NULL,
    "ReviewDate" TEXT NOT NULL,
    "Rating" INTEGER NOT NULL,
    "NextReviewDate" TEXT NOT NULL,
    CONSTRAINT "FK_Reviews_Sentences_SentenceId" FOREIGN KEY ("SentenceId") REFERENCES "Sentences" ("Id") ON DELETE CASCADE
);

-- Indexes
CREATE INDEX IF NOT EXISTS "IX_Sentences_EnglishSentence" ON "Sentences" ("EnglishSentence");
CREATE INDEX IF NOT EXISTS "IX_Sentences_NextReviewDate" ON "Sentences" ("NextReviewDate");
CREATE INDEX IF NOT EXISTS "IX_Sentences_MasteryScore" ON "Sentences" ("MasteryScore");
CREATE INDEX IF NOT EXISTS "IX_Sentences_DifficultyLevel" ON "Sentences" ("DifficultyLevel");
CREATE INDEX IF NOT EXISTS "IX_Reviews_SentenceId" ON "Reviews" ("SentenceId");
CREATE INDEX IF NOT EXISTS "IX_SentenceGroupLinks_SentenceId" ON "SentenceGroupLinks" ("SentenceId");
CREATE INDEX IF NOT EXISTS "IX_SentenceGroupLinks_GroupId" ON "SentenceGroupLinks" ("GroupId");

-- Seed data
INSERT OR IGNORE INTO "Users" ("Id", "Name", "CreatedDate") VALUES (1, 'Learner', datetime('now'));
INSERT OR IGNORE INTO "SentenceGroups" ("Id", "GroupName", "Description", "CreatedDate") VALUES 
    (1, 'Movies', 'Sentences from movies and TV shows', datetime('now')),
    (2, 'Daily English', 'Everyday conversational English', datetime('now')),
    (3, 'Special Education', 'Special education terminology', datetime('now')),
    (4, 'ABA', 'Applied Behavior Analysis terms', datetime('now')),
    (5, 'Interviews', 'Job interview preparation', datetime('now')),
    (6, 'Travel', 'Travel and tourism phrases', datetime('now'));
