using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace subleq_genetic
{
	class Program
	{
		Random random = new Random();
		const int ProgramLength = 256;
		const int PopulationSize = 1024;
		const int Generations = 1024 * 256;
		const float Culling = 0.5f;
		const int MaxExecutionTicks = 512;
		const int MaxOutputLength = 16;
		const string WantedResult = "Hello World!";
		const float TournamentSize = 0.25f;
		const float TournamentProbability = 0.1f;
		const float MutationProbability = 0.05f;

		static void Main(string[] args)
		{
			var p = new Program();
			p.Start();
		}

		class Fitness : IComparer<KeyValuePair<byte[], string>>
		{
			public int Compare(KeyValuePair<byte[], string> x, KeyValuePair<byte[], string> y)
			{
				return Levenshtein(x.Value, WantedResult).CompareTo(Levenshtein(y.Value, WantedResult));
			}
		}

		public void Start()
		{
			var fitness = new Fitness();
			var fittest = new List<byte[]>(PopulationSize);
			List<byte[]> population = new List<byte[]>(Enumerable.Range(0, PopulationSize)
				.Select((i) => RandomProgram()));
			List<byte[]> nextPopulation = new List<byte[]>(Enumerable.Range(0, PopulationSize).Select((i) => new byte[ProgramLength * 2]));
			List<byte[]> breeding = new List<byte[]>();

			List<KeyValuePair<byte[], string>> results = new List<KeyValuePair<byte[], string>>(Enumerable.Range(0, PopulationSize).Select((i) => new KeyValuePair<byte[], string>()));

			var genStringCount = Generations.ToString().Length;

			for (int gen = 0; gen < Generations; ++gen)
			{
				for (int i = 0; i < PopulationSize; ++i)
				{
					var bytes = population[i];
					results[i] = new KeyValuePair<byte[], string>(bytes, Execute(bytes));
				}

				results.Sort(fitness);
				if (gen % 100 == 0)
				{
					Console.WriteLine("[{0}] \"{1}\", \"{2}\", \"{3}\", \"{4}\"", gen.ToString().PadLeft(genStringCount), results[0].Value, results[1].Value, results[2].Value, results[3].Value);
				}

				fittest.Clear();
				for (int i = 0; i < (int)(PopulationSize * Culling); ++i)
				{
					if (random.NextDouble() < TournamentSize)
					{
						fittest.Add(results[i].Key);
					}
				}

				int currIndividual = 0;
				while(fittest.Count > 1 && currIndividual < (TournamentSize * (float)PopulationSize))
				{
					breeding.Clear();

					while (breeding.Count < 2)
					{
						for (int j = 0; j < fittest.Count && breeding.Count < 2; ++j)
						{
							var p = TournamentProbability * (Math.Pow((1 - TournamentProbability), j));
							if (random.NextDouble() < p)
							{
								breeding.Add(fittest[j]);
								//fittest.RemoveAt(j);
								//j--;
							}
						}
					}

					byte[] next = nextPopulation[currIndividual];
					for (int i = 0; i < ProgramLength * 2; ++i)
					{
						next[i] = breeding[(random.Next(0, 2))][i];
						if (random.NextDouble() < MutationProbability)
						{
							next[i] = (byte)random.Next(0, 256);
						}
					}
					currIndividual++;
				}
				int fittInd = 0;
				while (currIndividual < PopulationSize)
				{
					nextPopulation[currIndividual] = results[fittInd].Key;
					fittInd++;
					currIndividual++;
				}

				var tmp = nextPopulation;
				nextPopulation = population;
				population = tmp;
            }
			for (int i = 0; i < PopulationSize; ++i)
			{
				var bytes = nextPopulation[i];
				results[i] = new KeyValuePair<byte[], string>(bytes, Execute(bytes));
			}

			results.Sort(fitness);
			Console.WriteLine("Done!");
			Console.Read();
			Console.WriteLine(ToAssembly(results[0].Key));
			Console.Read();
		}

		public string ToAssembly(byte[] program)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < ProgramLength; i++)
			{
				builder.AppendLine(string.Format("{0}: subleq {1} {2}", i.ToString().PadLeft(ProgramLength.ToString().Length, '0'), program[i * 2], program[i * 2 + 1]));
			}
			return builder.ToString();
		}

		public byte[] RandomProgram()
		{
			byte[] ret = new byte[ProgramLength * 2];
			random.NextBytes(ret);
			return ret;
		}

		char[] output = new char[MaxOutputLength];
		public string Execute(byte[] program)
		{
			int outputIndex = 0;
			var memory = program.ToArray();
			int ip = 0;
			sbyte accum = 0;

			for (int i = 0; i < MaxExecutionTicks && outputIndex < MaxOutputLength; ++i)
			{
				var ma = unchecked((sbyte)memory[ip * 2]);
				ma = (sbyte)((int)ma - (int)accum);
				accum = ma;
				memory[ip] = unchecked((byte)ma);
				if (ma <= 0)
					ip = memory[(ip * 2 + 1) % (ProgramLength - 1)];
				else
					ip = (byte)((ip + 2) % (ProgramLength - 1));

				var mo = unchecked((sbyte)memory[0]);
				if (mo > 0)
				{
					if (mo < ' ') mo = (sbyte)' ';
					output[outputIndex] = Convert.ToChar(mo);
					outputIndex++;
				}
				else if (mo < 0)
				{
					break;
				}

				// This should be the same in both sbyte and byte
				memory[0] = 0;
			}
			return new string(output, 0, outputIndex);
		}

		/// <summary>
		/// Compute the distance between two strings.
		/// </summary>
		public static int Levenshtein(string s, string t)
		{
			int n = s.Length;
			int m = t.Length;
			int[,] d = new int[n + 1, m + 1];

			// Step 1
			if (n == 0)
			{
				return m;
			}

			if (m == 0)
			{
				return n;
			}

			// Step 2
			for (int i = 0; i <= n; d[i, 0] = i++)
			{
			}

			for (int j = 0; j <= m; d[0, j] = j++)
			{
			}

			// Step 3
			for (int i = 1; i <= n; i++)
			{
				//Step 4
				for (int j = 1; j <= m; j++)
				{
					// Step 5
					int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

					// Step 6
					d[i, j] = Math.Min(
						Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
						d[i - 1, j - 1] + cost);
				}
			}
			// Step 7
			return d[n, m];
		}
	}
}
