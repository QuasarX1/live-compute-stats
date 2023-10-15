using System.Drawing;

namespace display_stats.Data
{
    public class SlurmQueueColours
    {
        private Dictionary<string, System.Drawing.Color> _colours = new Dictionary<string, Color>();

        public int Length => _colours.Count;
        public string[] QueueNames => _colours.Keys.ToArray();

        public SlurmQueueColours() { }

        /// <summary>
        ///
        /// </summary>
        /// <param name="queue_names"></param>
        /// <param name="colours">Formats: "#RGB", "#RRGGBB", "#RGBA", "#RRGGBBAA"</param>
        /// <exception cref="ArgumentException"></exception>
        public SlurmQueueColours(string[] queue_names, string[] colours)
        {
            if (queue_names.Length != colours.Length)
            {
                throw new ArgumentException("Arrays not of the same length.");
            }

            for (int i = 0; i < queue_names.Length; i++)
            {
                Color colour;
                if (colours[i][0] == '#')
                {
                    switch (colours[i].Length)
                    {
                        case 4:
                            colour = Color.FromArgb(System.Convert.ToInt32(System.Convert.FromHexString($"0{colours[i][1]}")[0]),
                                                    System.Convert.ToInt32(System.Convert.FromHexString($"0{colours[i][2]}")[0]),
                                                    System.Convert.ToInt32(System.Convert.FromHexString($"0{colours[i][3]}")[0]));
                            break;
                        case 5:
                            colour = Color.FromArgb(System.Convert.ToInt32(System.Convert.FromHexString($"0{colours[i][1]}")[0]),
                                                    System.Convert.ToInt32(System.Convert.FromHexString($"0{colours[i][2]}")[0]),
                                                    System.Convert.ToInt32(System.Convert.FromHexString($"0{colours[i][3]}")[0]),
                                                    System.Convert.ToInt32(System.Convert.FromHexString($"0{colours[i][4]}")[0]));
                            break;
                        case 7:
                            colour = Color.FromArgb(System.Convert.ToInt32(System.Convert.FromHexString(colours[i][1..3])[0]),
                                                    System.Convert.ToInt32(System.Convert.FromHexString(colours[i][3..5])[0]),
                                                    System.Convert.ToInt32(System.Convert.FromHexString(colours[i][5..7])[0]));
                            break;
                        case 9:
                            colour = Color.FromArgb(System.Convert.ToInt32(System.Convert.FromHexString(colours[i][1..3])[0]),
                                                    System.Convert.ToInt32(System.Convert.FromHexString(colours[i][3..5])[0]),
                                                    System.Convert.ToInt32(System.Convert.FromHexString(colours[i][5..7])[0]),
                                                    System.Convert.ToInt32(System.Convert.FromHexString(colours[i][7..9])[0]));
                            break;
                        default:
                            throw new ArgumentException("Invalid colour format.");
                    }
                }
                else
                {
                    colour = Color.FromName(colours[i]);
                }
                _colours.Add(queue_names[i], colour);
            }
        }

        public Color GetColour(string queue_name)
        {
            return _colours[queue_name];
        }
    }
}
