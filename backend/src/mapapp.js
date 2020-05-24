class Map {
  constructor(mapid) {
    let map = L.map(mapid);
    map.on('zoomend',  async e => onChange(this, e));
    map.on('load', async e => onChange(this, e));
    map.on('moveend', async e => onChange(this, e));
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
    
    this._map = map
    this._markers = {}
  }

  setMarkers(result) {
    let newMarkers = {}
    let deleteMarkers = this._markers
    for (let i = 0; i < result.length; ++i) {
      let item = result[i];
      if (item.id in this._markers) {
        newMarkers[item.id] = this._markers[item.id]
        delete deleteMarkers[item.id]
      } else {
        console.log("Creating marker " + item.lat + ", " + item.lng + " with title " + item.title)
        let marker = L.marker([item.lat, item.lng]).addTo(map._map);
        marker.bindPopup(item.title)
        newMarkers[item.id] = marker
      }
      // L.popup().setLatLng(L.latlng(item["lat"], item["lng"]))
    }
    for (let [id, marker] of Object.entries(deleteMarkers)) {
      console.log("Removing ID " + id)
      marker.remove()
    }
    console.log("All markers: ")
    for (let [id, marker] of Object.entries(newMarkers)) {
      console.log(id)
    } 
    this._markers = newMarkers
  }
}

async function onChange(map, e) {
    console.log("Change: " + JSON.stringify(map._map.getBounds()));
    let response = await fetch("http://localhost:5000/events", {
      method: 'POST',
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify(map._map.getBounds())
    })
    let result = await response.json()
    console.log("Data: " + JSON.stringify(result));
    map.setMarkers(result)
}

// var map = new Map("map")
function initializeMap(mapid)
{
  return new Map(mapid)
}