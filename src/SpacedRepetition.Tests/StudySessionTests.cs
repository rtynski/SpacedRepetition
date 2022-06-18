using SpacedRepetition.Net.ReviewStrategies;
using System;
using System.Linq;
using Xunit;

namespace SpacedRepetition.Net.Tests.Unit
{
    public class StudySessionTests
    {
        private const int _maxNewCardsPerSession = 5;
        private const int _maxExistingCardsPerSession = 7;
        private readonly ClockStub _clock = new ClockStub(DateTime.Now);

        [Theory]
        [InlineData(ReviewOutcome.Perfect)]
        [InlineData(ReviewOutcome.Hesitant)]
        public void correct_review_outcome_increments_CorrectReviewStreak(ReviewOutcome outcome)
        {
            var correctReviewStreak = 3;
            var item = new ReviewItemBuilder().Due().WithCorrectReviewStreak(correctReviewStreak).Build();

            var session = new StudySession<ReviewItem>(new[] { item });
            var review = session.Review(item, outcome);

            Assert.Equal(correctReviewStreak + 1, review.CorrectReviewStreak);
        }

        [Theory]
        [InlineData(ReviewOutcome.Perfect)]
        [InlineData(ReviewOutcome.Hesitant)]
        public void correct_review_outcome_sets_PreviousCorrectReview(ReviewOutcome outcome)
        {
            var correctReviewStreak = 3;
            var item = new ReviewItemBuilder().Due().WithCorrectReviewStreak(correctReviewStreak).Build();
            var reviewDate = item.ReviewDate;

            var session = new StudySession<ReviewItem>(new[] { item });
            var review = session.Review(item, outcome);

            Assert.Equal(reviewDate, review.PreviousCorrectReview);
        }

        [Fact]
        public void incorrect_review_resets_CorrectReviewStreak()
        {
            var correctReviewStreak = 3;
            var item = new ReviewItemBuilder().Due().WithCorrectReviewStreak(correctReviewStreak).Build();

            var session = new StudySession<ReviewItem>(new[] { item });
            var review = session.Review(item, ReviewOutcome.Incorrect);

            Assert.Equal(0, review.CorrectReviewStreak);
        }

        [Fact]
        public void incorrect_review_resets_PreviousReviewDate()
        {
            var correctReviewStreak = 3;
            var item = new ReviewItemBuilder().Due().WithCorrectReviewStreak(correctReviewStreak).Build();

            var session = new StudySession<ReviewItem>(new[] { item });
            var review = session.Review(item, ReviewOutcome.Incorrect);

            Assert.Equal(DateTime.MinValue, review.PreviousCorrectReview);
        }

        [Theory]
        [InlineData(ReviewOutcome.Perfect)]
        [InlineData(ReviewOutcome.Hesitant)]
        [InlineData(ReviewOutcome.Incorrect)]
        public void reviewing_updates_LastReviewDate_to_now(ReviewOutcome outcome)
        {
            var item = new ReviewItemBuilder().Due().Build();

            var session = new StudySession<ReviewItem>(new[] {item}) {Clock = _clock};
            var review = session.Review(item, outcome);

            Assert.Equal(_clock.Now(), review.ReviewDate);
        }

        [Theory]
        [InlineData(ReviewOutcome.Perfect)]
        [InlineData(ReviewOutcome.Hesitant)]
        [InlineData(ReviewOutcome.Incorrect)]
        public void reviewing_updates_DifficultyRating_based_on_review_strategy(ReviewOutcome outcome)
        {
            var item = new ReviewItemBuilder().Due().WithDifficultyRating(DifficultyRating.MostDifficult).Build();

            var session = new StudySession<ReviewItem>(new[] { item }) { ReviewStrategy = new SimpleReviewStrategy() };
            var review = session.Review(item, outcome);

            Assert.Equal(DifficultyRating.Easiest, review.DifficultyRating);
        }

        [Theory]
        [InlineData(ReviewOutcome.Perfect)]
        [InlineData(ReviewOutcome.Hesitant)]
        public void correct_items_are_removed_from_review_queue(ReviewOutcome outcome)
        {
            var items = new ReviewItemListBuilder()
                            .WithDueItems(1)
                            .Build();
            var session = new StudySession<ReviewItem>(items);

            var item = session.First();
            session.Review(item, outcome);

            Assert.Empty(session);
        }

        [Fact]
        public void incorrect_items_stay_in_review_queue()
        {
            var items = new ReviewItemListBuilder()
                            .WithDueItems(1)
                            .Build();
            var session = new StudySession<ReviewItem>(items);

            var item = session.First();
            var review = session.Review(item, ReviewOutcome.Incorrect);

            Assert.Equal(review, session.First());
        }

        [Fact]
        public void incorrect_items_stay_in_review_queue_until_correct()
        {
            var items = new ReviewItemListBuilder()
                            .WithDueItems(1)
                            .Build();
            var session = new StudySession<ReviewItem>(items);

            var incorrectTimes = 0;
            foreach (var reviewItem in session)
            {
                if (incorrectTimes++ < 3)
                    session.Review(reviewItem, ReviewOutcome.Incorrect);
                else break;
            }

            session.Review(session.First(), ReviewOutcome.Perfect);

            Assert.Empty(session);
        }

        [Fact]
        public void only_return_due_items()
        {
            var dueItems = 2;
            var items = new ReviewItemListBuilder()
                            .WithDueItems(dueItems)
                            .WithFutureItems(3)
                            .Build();
            var session = new StudySession<ReviewItem>(items);

            Assert.Equal(dueItems, session.Count());
        }

        [Fact]
        public void limit_new_cards_per_session()
        {
            var items = new ReviewItemListBuilder().WithNewItems(_maxNewCardsPerSession + 1).Build();
            var session = new StudySession<ReviewItem>(items) { MaxNewCards = _maxNewCardsPerSession };

            Assert.Equal(_maxNewCardsPerSession, session.Count());
        }

        [Fact]
        public void limit_new_cards_when_there_are_also_existing_cards()
        {
            var items = new ReviewItemListBuilder()
                .WithNewItems(_maxNewCardsPerSession - 1)
                .WithExistingItems(1)
                .WithNewItems(2)
                .Build();

            var session = new StudySession<ReviewItem>(items) { MaxNewCards = _maxNewCardsPerSession };

            Assert.Equal(_maxNewCardsPerSession, session.Count(x => x.ReviewDate == DateTime.MinValue));
        }

        [Fact]
        public void limit_existing_cards_per_session()
        {
            var items = new ReviewItemListBuilder().WithExistingItems(_maxExistingCardsPerSession + 1).Build();
            var session = new StudySession<ReviewItem>(items) { MaxExistingCards = _maxExistingCardsPerSession };

            Assert.Equal(_maxExistingCardsPerSession, session.Count());
        }

        [Fact]
        public void limit_existing_cards_when_there_are_also_new_cards()
        {
            var items = new ReviewItemListBuilder()
                               .WithExistingItems(_maxExistingCardsPerSession - 1)
                               .WithNewItems(1)
                               .WithExistingItems(2)
                               .Build();

            var session = new StudySession<ReviewItem>(items) { MaxExistingCards = _maxExistingCardsPerSession};

            Assert.Equal(_maxExistingCardsPerSession, session.Count(x => x.ReviewDate != DateTime.MinValue));
        }

        [Fact]
        public void item_is_due_for_review()
        {
            var items = new ReviewItemListBuilder()
                   .WithNewItems(1)
                   .Build();

            var session = new StudySession<ReviewItem>(items);

            Assert.True(session.IsDue(items.First()));

        }

        [Fact]
        public void item_is_not_due_for_review()
        {
            var items = new ReviewItemListBuilder()
                   .WithFutureItems(1)
                   .Build();

            var session = new StudySession<ReviewItem>(items);

            Assert.False(session.IsDue(items.First()));

        }
    }
}
