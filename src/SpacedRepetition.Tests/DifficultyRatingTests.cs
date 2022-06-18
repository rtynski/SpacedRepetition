using Xunit;

namespace SpacedRepetition.Net.Tests.Unit
{
    public class DifficultyRatingTests
    {
        [Fact]
        public void implicitly_convert_from_difficulty_rating_to_int()
        {
            var difficultyRating = new DifficultyRating(50);

            int percentage = difficultyRating;
            Assert.Equal(difficultyRating.Percentage, percentage);
        }

        [Fact]
        public void implicitly_convert_from_int_to_difficulty_rating()
        {
            int percentage = 65;
            DifficultyRating difficultyRating = percentage;

            Assert.Equal(percentage, difficultyRating.Percentage);
        }

        [Fact]
        public void implicitly_convert_from_difficulty_rating_to_byte()
        {
            var difficultyRating = new DifficultyRating(50);

            byte percentage = difficultyRating;
            Assert.Equal(difficultyRating.Percentage, percentage);
        }

        [Fact]
        public void implicitly_convert_from_byte_to_difficulty_rating()
        {
            byte percentage = 65;
            DifficultyRating difficultyRating = percentage;

            Assert.Equal(percentage, difficultyRating.Percentage);
        }
    }
}
