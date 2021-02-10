using System;
using System.Linq;

public class Program
{
	public static void Main()
	{
		Random r = new Random();

		double RandomNormal(double min, double max, int tightness)
		{
			double total = 0.0;
			for (int i = 1; i <= tightness; i++)
			{
				total += r.NextDouble();
			}
			return ((total / tightness) * (max - min)) + min;
		}

		double RandomNormalDist(double min, double max, int tightness, double exp)
		{
			double total = 0.0;
			for (int i = 1; i <= tightness; i++)
			{
				total += Math.Pow(r.NextDouble(), exp);
			}

			return ((total / tightness) * (max - min)) + min;
		}


		double RandomBiasedPow(double min, double max, int tightness, double peak)
		{
			// Calculate skewed normal distribution, skewed by Math.Pow(...), specifiying where in the range the peak is
			// NOTE: This peak will yield unreliable results in the top 20% and bottom 20% of the range.
			//       To peak at extreme ends of the range, consider using a different bias function

			double total = 0.0;
			double scaledPeak = peak / (max - min) + min;

			if (scaledPeak < 0.2 || scaledPeak > 0.8)
			{
				throw new Exception("Peak cannot be in bottom 20% or top 20% of range.");
			}

			double exp = GetExp(scaledPeak);

			for (int i = 1; i <= tightness; i++)
			{
				// Bias the random number to one side or another, but keep in the range of 0 - 1
				// The exp parameter controls how far to bias the peak from normal distribution
				total += BiasPow(r.NextDouble(), exp);
			}

			return ((total / tightness) * (max - min)) + min;
		}

		double GetExp(double peak)
		{
			// Get the exponent necessary for BiasPow(...) to result in the desired peak 
			// Based on empirical trials, and curve fit to a cubic equation, using WolframAlpha
			return -12.7588 * Math.Pow(peak, 3) + 27.3205 * Math.Pow(peak, 2) - 21.2365 * peak + 6.31735;
		}

		double BiasPow(double input, double exp)
		{
			return Math.Pow(input, exp);
		}

		public static int[] Permutation(this Random rand, int n, int k)
		{
			var result = new List<int>();
			var sorted = new SortedSet<int>();

			for (var i = 0; i < k; i++)
			{
				var r = rand.Next(1, n + 1 - i);

				foreach (var q in sorted)
					if (r >= q) r++;

				result.Add(r);
				sorted.Add(r);
			}

			return result.ToArray();
		}
	}


	static double gaus(this Random r, double mu = 0, double sigma = 1)
	{
		var u1 = r.NextDouble();
		var u2 = r.NextDouble();

		var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) *
							Math.Sin(2.0 * Math.PI * u2);

		var rand_normal = mu + sigma * rand_std_normal;

		return rand_normal;
	}

	/*

	public static double gaus(this Random r, double mu = 0, double sigma = 1)
	{
		var u1 = r.NextDouble();
		var u2 = r.NextDouble();

		var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) *
			Math.Sin(2.0 * Math.PI * u2);

		var rand_normal = mu + sigma * rand_std_normal;
		
		return rand_normal;
	}
	// between 0-100 distribution 
	public static void Main()
	{
		var r = new Random();
		var _ret = r.gaus() * (50/3);
		
		Console.WriteLine(_ret+50);
	}
		
	In response to your comment, the rule of thumb is that 99.7% of a normal distribution will be within +/- 3 times the standard deviation. 
	If you need a normal distribution from 0 to 100 for instance, than your mean will be halfway, and your SD will be (100/2)/3 = 16.667. 
	So whatever values you get out of your Box-Muller algorithm, multiply by 16.667 to "stretch" the distribution out, then add 50 to "center" it.
	*/
}