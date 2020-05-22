class Map {
  constructor(mapid) {
    let map = L.map('map');
    map.on('zoomend',  async e => onChange(map, e));
    map.on('load', async e => onChange(map, e));
    map.on('moveend', async e => onChange(map, e));
    map.setView({lon: 0, lat: 0}, 2);
    // add the OpenStreetMap tiles
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; <a href="https://openstreetmap.org/copyright">OpenStreetMap contributors</a>'
    }).addTo(map);
    
    // show the scale bar on the lower left corner
    L.control.scale().addTo(map);
    
    // show a marker on the map
    L.marker({lon: 0, lat: 0}).bindPopup('The center of the world').addTo(map);
    
    // var popup = L.popup();
    map.on('click', this.onChange);
    this.map = map
  }
  
}
async function onChange(map, e) {
    console.log("Change: " + JSON.stringify(map.getBounds()));
    let response = await fetch("http://localhost:5000/events", {
      method: 'POST',
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify(map.getBounds())
    })
    let result = await response.json()
    console.log("Data: " + JSON.stringify(result));
    for (var i = 0; i < result.length; ++i) {
      var item = result[i];
      console.log("Creating marker " + item["lat"] + ", " + item["lng"] + " with title " + item["title"])
      L.marker([item["lat"], item["lng"]]).addTo(map).bindPopup(item["title"])
      // L.popup().setLatLng(L.latlng(item["lat"], item["lng"]))
    }
}

function onMapClick(e) {
  popup
      .setLatLng(e.latlng)
      .setContent("You clicked the map at " + e.latlng.toString())
      .openOn(map);
}
// function initializeMap(mapid) {
  
// }

// initializeMap("map")
var map = new Map("map")