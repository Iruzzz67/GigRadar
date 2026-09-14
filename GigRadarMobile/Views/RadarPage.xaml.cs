using System.Text.Json;
using GigRadarMobile.ViewModels;
using Microsoft.Maui.ApplicationModel;

namespace GigRadarMobile.Views;

public partial class RadarPage : ContentPage
{
    private readonly RadarViewModel _viewModel;

    public RadarPage(RadarViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(RadarViewModel.CenterLat)
                or nameof(RadarViewModel.CenterLng)
                or nameof(RadarViewModel.RadiusKm)
                or nameof(RadarViewModel.Events))
            {
                MainThread.BeginInvokeOnMainThread(UpdateMap);
            }
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnPageAppearing();
                UpdateMap();
    }

        private void UpdateMap()
    {
                var markers = _viewModel.Events.Select(eventItem => new
                {
                        id = eventItem.EventId,
                        lat = eventItem.Latitude,
                        lng = eventItem.Longitude,
                        name = eventItem.Name,
                        venue = eventItem.VenueName
                });

                var markerJson = JsonSerializer.Serialize(markers);
                MapWebView.Source = new HtmlWebViewSource
                {
                        Html = BuildMapHtml(markerJson)
                };
        }

        private string BuildMapHtml(string markerJson)
        {
            var html = """
                        <!doctype html>
                        <html>
                        <head>
                            <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no" />
                            <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
                            <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
                            <style>
                                html, body, #map { height: 100%; margin: 0; background: #18181b; }
                                .leaflet-control-attribution { font-size: 9px; }
                                .gig-marker { background: #a3ff12; border: 3px solid #0a0a0b; border-radius: 50%; width: 16px; height: 16px; box-shadow: 0 0 0 2px #a3ff12; }
                                .user-marker { background: #2389ff; border: 3px solid white; border-radius: 50%; width: 14px; height: 14px; box-shadow: 0 0 0 5px rgba(35,137,255,.25); }
                            </style>
                        </head>
                        <body>
                            <div id="map"></div>
                            <script>
                                const center = [__LAT__, __LNG__];
                                const radiusKm = __RADIUS__;
                                const events = __EVENTS__;
                                const map = L.map('map', { zoomControl: true, attributionControl: true }).setView(center, radiusKm <= 10 ? 12 : radiusKm <= 25 ? 11 : 10);
                                L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', { maxZoom: 19, subdomains: 'abcd', attribution: '&copy; <a href=\"https://www.openstreetmap.org/copyright\">OpenStreetMap</a> &copy; <a href=\"https://carto.com/attributions\">CARTO</a>' }).addTo(map);
                                L.marker(center, { icon: L.divIcon({ className: 'user-marker', iconSize: [14, 14], iconAnchor: [7, 7] }) }).addTo(map).bindTooltip('Lokasi kamu');
                                L.circle(center, { radius: radiusKm * 1000, color: '#a3ff12', weight: 1, fillColor: '#a3ff12', fillOpacity: .06 }).addTo(map);
                                events.forEach(eventItem => {
                                    const marker = L.marker([eventItem.lat, eventItem.lng], { icon: L.divIcon({ className: 'gig-marker', iconSize: [16, 16], iconAnchor: [8, 8] }) }).addTo(map);
                                    marker.bindPopup('<strong>' + escapeHtml(eventItem.name) + '</strong><br>' + escapeHtml(eventItem.venue));
                                    marker.on('click', () => window.location.href = 'gigradar://event/' + eventItem.id);
                                });
                                function escapeHtml(value) { return String(value).replace(/[&<>'"]/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[character]); }
                            </script>
                        </body>
                        </html>
                        """;

                    return html
                        .Replace("__LAT__", _viewModel.CenterLat.ToString(System.Globalization.CultureInfo.InvariantCulture))
                        .Replace("__LNG__", _viewModel.CenterLng.ToString(System.Globalization.CultureInfo.InvariantCulture))
                        .Replace("__RADIUS__", _viewModel.RadiusKm.ToString(System.Globalization.CultureInfo.InvariantCulture))
                        .Replace("__EVENTS__", markerJson);
        }

        private void OnMapNavigating(object? sender, WebNavigatingEventArgs e)
        {
                if (!e.Url.StartsWith("gigradar://event/", StringComparison.OrdinalIgnoreCase))
                        return;

                e.Cancel = true;
                if (int.TryParse(e.Url["gigradar://event/".Length..], out var eventId))
                        _viewModel.SelectEventById(eventId);
    }
}