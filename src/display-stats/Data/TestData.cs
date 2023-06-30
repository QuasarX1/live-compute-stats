namespace display_stats.Data
{
    public class TestData
    {
        public int Value { get; private set; }
        public TestData()
        {
            var r = new System.Random();
            Value = r.Next();
        }
    }
}
