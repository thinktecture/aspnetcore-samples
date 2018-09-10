using FlightFinder.Shared;
using Microsoft.AspNetCore.Blazor;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Blazor.Extensions.Storage.Interfaces;

namespace FlightFinder.Client.Services
{
    public class AppState
    {
        // Actual state
        public IReadOnlyList<Itinerary> SearchResults { get; private set; }
        public bool SearchInProgress { get; private set; }

        private readonly List<Itinerary> shortlist = new List<Itinerary>();
        public IReadOnlyList<Itinerary> Shortlist => shortlist;

        // Lets components receive change notifications
        // Could have whatever granularity you want (more events, hierarchy...)
        public event Action OnChange;

        // Receive 'http' instance from DI
        private readonly HttpClient http;
        private readonly IStorage _storage;

        public AppState(HttpClient httpInstance, IStorage storage)
        {
            http = httpInstance;
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));

            LoadShortlist();
        }

        public async Task Search(SearchCriteria criteria)
        {
            SearchInProgress = true;
            NotifyStateChanged();

            SearchResults = await http.PostJsonAsync<Itinerary[]>("/api/flightsearch", criteria);
            SearchInProgress = false;
            NotifyStateChanged();
        }

        public void AddToShortlist(Itinerary itinerary)
        {
            shortlist.Add(itinerary);
            StoreShortlist();
            NotifyStateChanged();
        }

        public void RemoveFromShortlist(Itinerary itinerary)
        {
            shortlist.Remove(itinerary);
            StoreShortlist();
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        private void StoreShortlist()
        {
            _storage.SetItem("shortlist", shortlist);
        }

        private async Task LoadShortlist()
        {
            var list = await _storage.GetItem<List<Itinerary>>("shortlist");

            shortlist.AddRange(list);
            NotifyStateChanged();
        }
    }
}
