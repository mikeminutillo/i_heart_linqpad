<Query Kind="Program" />

void Main()
{
	var start = DateTime.Today.AddDays(-2);
	// Still Watching
	Add("Bored to Death", Episodes(8));
	Add("Chuck", Season(2));
	Add("Community", Episodes(20));
	Add("Human Target", Episodes(12));
	Add("Parenthood", Episodes(13));

	// Now Showing
	Add("Futurama", Season(6), Episodes(26), Ep(1, Jun(24)), Ep(2, Jun(24)));
	Add("Happy Town", Episodes(8), Ep(4, Jun(2)), Ep(7, Jun(30)));
	Add("Persons Unknown", Episodes(13), Ep(1, Jun(7)));
	Add("The Gates", Episodes(13), Ep(1, Jun(20)), Ep(3, Jul(11)));
	Add("The IT Crowd", Episodes(6), Season(4), Ep(1, Jun(25)));
	Add("Through the Wormhole", Episodes(8), Ep(1, Jun(9))); 

	// Coming soon
	Add("Haven", Episodes(13), Ep(1, Jul(9)));
	Add("Warehouse 13", Season(2), Episodes(13), Ep(1, Jul(13)));
	Add("Venture Brothers", Season(4), Episodes(16), Ep(9, Aug(22)));
	Add("Caprica", Episodes(20), Ep(10, Sep(17)));
	Add("Stargate Universe", Season(2), Episodes(20), Ep(1, Sep(28)));
	Add("The Cape");
	Add("Undercovers");
	Add("No Ordinary Family");
	
	// Next Season
	Add("Doctor Who", Season(6));
	Add("Fringe", Season(3), Episodes(22)); // Aug/Sep
	Add("Big Bang Theory", Season(4)); // Sep
	Add("Castle", Episodes(22), Season(3)); // Sep
	Add("Grey's Anatomy", Season(7)); // Sep
	Add("Glee", Season(2), Episodes(22)); // Sep
	Add("Gossip Girl", Season(4)); // Sep
	Add("V", Episodes(13), Season(2)); // Nov
	Add("Luther", Episodes(6), Season(2)); // ?


	_shows.SelectMany(x => x.GetEpisodes()).Where(x => x.AirDate.HasValue).OrderBy(x => x.AirDate).SkipWhile(x => x.AirDate.Value < start)
		  .GroupBy(x => String.Format("{0:MMM}", x.AirDate) )
		  .Select(x => new { Month = x.Key, 
			                 Days = x.GroupBy(y => y.AirDate)
							 		 .Select(y => new { 
									 					//y.Key.Value.DayOfWeek, 
														y.Key.Value.Day, 
														Episodes = String.Join(", ", y.Select(z => z.ToString()).ToArray()),
														NextDay = y.Key.Value.AddDays(1).DayOfWeek,
														Additional = String.Join(", ", y.Select(z => z.AdditionalInfo).Where(z => !String.IsNullOrEmpty(z)).ToArray())
													  }
									  )
							}
		   ).Dump("Schedule");
	_shows.Select(x => new { Show = x.ToString(), LastEpDate = x.GetEpisodes().Last().AirDate })
		  .Where(x => x.LastEpDate.GetValueOrDefault(DateTime.MinValue) < start)
		  .Dump("Finished");
}

// Define other methods and classes here
List<Show> _shows = new List<Show>();

void Add(string name, params ShowMod[] mods)
{
	var show = new Show { Name = name };
	foreach(var mod in mods) { mod.ApplyTo(show); }
	_shows.Add(show);
}

ShowMod Season(int season)
{
	return new DelegateShowMod(s => s.Season = season);
}

ShowMod Episodes(int episodes)
{
	return new DelegateShowMod(s => s.EpisodeCount = episodes);
}

ShowMod Ep(int num, DateTime airDate)
{
	return new DelegateShowMod(s => s.KnownDates[num] = airDate);
}


interface ShowMod
{
	void ApplyTo(Show show);
}

class Show
{
	public Show()
	{
		Season = 1;
		EpisodeCount = 1;
		KnownDates = new Dictionary<int, DateTime>();
	}

	public string Name;
	public int Season;
	public int EpisodeCount;
	public IDictionary<int, DateTime> KnownDates;
	
	public override string ToString()
	{
		return String.Format("{0} season {1}", Name, Season);
	}
	
	public IEnumerable<Episode> GetEpisodes()
	{
		var episodes = Enumerable.Range(1, EpisodeCount).Select(x => new Episode { Show = this, EpisodeNumber = x }).ToList();
		
		DateTime? prevEp = null;
		foreach(var episode in episodes)
		{
			DateTime airDate;
			if(KnownDates.TryGetValue(episode.EpisodeNumber, out airDate))
			{
				episode.AirDate = airDate;
			}
			else if(prevEp.HasValue)
			{
				episode.AirDate = prevEp.Value.AddDays(7);
			}
			
			prevEp = episode.AirDate;
		}
		
		return episodes;
	}
}

class DelegateShowMod : ShowMod
{
	private Action<Show> _action;
	public DelegateShowMod(Action<Show> action) { this._action = action; }
	public void ApplyTo(Show show) { _action(show); }
}

class Episode
{
	public Show Show;
	public int EpisodeNumber;
	public DateTime? AirDate;
	
	public override string ToString()
	{
		return String.Format("{0} [{1}x{2:00}]{3}", Show.Name, Show.Season, EpisodeNumber, EpisodeNumber == Show.EpisodeCount ? "*" : "");
	}
	
	public string AdditionalInfo
	{
		get
		{
			if(EpisodeNumber == 1)
			{
				if(Show.Season == 1)
				{
					return "Pilot";
				}
				else
				{
					return "Season Starts";
				}
			}
			if(EpisodeNumber == Show.EpisodeCount)
			{
				return "Finale";
			}
			return "";
		}
	}
}

static DateTime Jan(int day){ return new DateTime(2010, 1, day); }
static DateTime Feb(int day){ return new DateTime(2010, 2, day); }
static DateTime Mar(int day){ return new DateTime(2010, 3, day); }
static DateTime Apr(int day){ return new DateTime(2010, 4, day); }
static DateTime May(int day){ return new DateTime(2010, 5, day); }
static DateTime Jun(int day){ return new DateTime(2010, 6, day); }
static DateTime Jul(int day){ return new DateTime(2010, 7, day); }
static DateTime Aug(int day){ return new DateTime(2010, 8, day); }
static DateTime Sep(int day){ return new DateTime(2010, 9, day); }
static DateTime Oct(int day){ return new DateTime(2010, 10, day); }
static DateTime Nov(int day){ return new DateTime(2010, 11, day); }
static DateTime Dec(int day){ return new DateTime(2010, 12, day); }