using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tgs.Payment.Extensions;

public static class EnumerableExtensions
{
	public static IList<T[]> ToArraySegments<T>(this IEnumerable<T> source, int limit)
	{
		if (limit < 1)
		{
			throw new ArgumentException($"limit for {nameof(ToArraySegments)} cannot be less than 1");
		}

		var arrays = new List<T[]>();
		var current = new List<T>(limit);

		foreach (var e in source)
		{
			current.Add(e);
			if (current.Count != limit)
			{
				continue;
			}

			arrays.Add(current.ToArray());
			current = new(limit);
		}

		if (current.Count > 0)
		{
			arrays.Add(current.ToArray());
		}

		return arrays;
	}

	public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> source) =>
			Task.WhenAll(source);

	public static ValueTask<T[]> WhenAll<T>(this IEnumerable<ValueTask<T>> tasks)
	{
		var list = tasks.ToList();
		var length = list.Count;
		var result = new T[length];
		var i = 0;

		for (; i < length; i++)
		{
			if (list[i].IsCompletedSuccessfully)
			{
				result[i] = list[i].Result;
			}
			else
			{
				return WhenAllAsync();
			}
		}

		return new ValueTask<T[]>(result);

		async ValueTask<T[]> WhenAllAsync()
		{
			for (; i < length; i++)
			{
				try
				{
					result[i] = await list[i];
				}
				catch
				{
					for (i++; i < length; i++)
					{
						try
						{
							await list[i];
						}
						catch
						{
							// ignored
						}
					}

					throw;
				}
			}

			return result;
		}
	}
}
