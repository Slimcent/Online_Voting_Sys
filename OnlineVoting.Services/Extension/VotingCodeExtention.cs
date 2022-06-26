namespace OnlineVoting.Services.Extension
{
    public static class VotingCodeExtention
    {
        public static string StudentVotingCode() 
        {
            Random random = new Random();

            // strings of Alphabets
            string lowerCase = "abcdefghijklmnopqrstuvwsyz";
            string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWSYZ";
            string numbers = "123456789";
            string finalResult = "";

            int size = 4;

            // initializing the empty strings
            string lowerCaseResult = "";
            string upperCaseResult = "";
            string numberResult = "";

            for (int i = 0; i < size; i++)
            {
                // selecting index randomly
                int lowerCaseIndex = random.Next(26);
                int upperCaseIndex = random.Next(26);
                int numberIndex = random.Next(9);

                // Appending the character at index to the random string
                lowerCaseResult += lowerCase[lowerCaseIndex];
                upperCaseResult += upperCase[upperCaseIndex];
                numberResult += numbers[numberIndex];

                //finalResult += lowerCase[lowerCaseIndex];
                //finalResult += upperCase[upperCaseIndex];
                //finalResult += numbers[numberIndex];
            }

            finalResult = lowerCaseResult + upperCaseResult + numberResult;

            string shuffleFinalResult = ShuffleExtension.Shuffle(finalResult);

            return shuffleFinalResult;
        }
    }

    static class ShuffleExtension
    {
        public static string Shuffle(this string result)
        {
            return new string(result.ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
        }
    }
}
