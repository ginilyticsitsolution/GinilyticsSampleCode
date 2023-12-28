using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Clubr.DTO.Responses;
using Microsoft.WindowsAzure.MobileServices;
using Clubr.Mobile.Services;
using Clubr.Res;
using System.Collections.ObjectModel;
using Clubr.Mobile.Repositories;

namespace Clubr.Mobile.ViewModels
{
	public class ClubListingsViewModel : ListViewModel<ClubListing>
	{
		private IEnumerable<ClubListing> _clubListings;

		public ObservableCollection<ClubListing> ClubListings { get; set; }

		public ClubListingsViewModel(IClubListingRepository clubListingRepository, IApp app, IDialogsService dialogsService,
			INavigationService navigationService, IInsightsService insightsService)
			: base(clubListingRepository, app, dialogsService, navigationService, insightsService)
		{
			Icon = "CityClubs.png";

			_clubListings = new List<ClubListing>();
		}

		protected override async Task ItemTappedAsync(ClubListing item)
		{
			await Navigation.NavigateToAsync<ClubListingDetailsViewModel>(item);
		}

		public override void Init(object args)
		{
			base.Init(args);

			var clubListings = args as IEnumerable<ClubListing>;
			if (clubListings != null)
			{
				ClubListings = clubListings.ToObservableCollection();
			}
		}
	}
}