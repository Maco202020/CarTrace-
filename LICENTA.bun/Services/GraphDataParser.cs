using System.Globalization;
using System.Text.RegularExpressions;

public static class GraphDataParser
{
	public static double? Parse(string valoareReala)
	{
		if (string.IsNullOrWhiteSpace(valoareReala)) return null;

		string textUpper = valoareReala.ToUpper();
		if (textUpper.Contains("NO DATA") || textUpper.Contains("ERROR") 
			||textUpper.Contains("?") || textUpper.Contains("STOPPED"))
		{
			return null;
		}

		Match match = Regex.Match(valoareReala, @"[-+]?[0-9]+([.,][0-9]+)?");

		if (match.Success)
		{
			string doarNumarul = match.Value.Replace(",", ".");
			if (double.TryParse(doarNumarul, NumberStyles.Any, CultureInfo.InvariantCulture, out double rezultat))
			{
				return rezultat;
			}
		}
		return null;
	}
}