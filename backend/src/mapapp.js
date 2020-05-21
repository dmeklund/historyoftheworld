class Map {
  constructor(mapid) {
    let map = L.map('map');
    map.on('zoomend',  e => onChange(map, e));
    map.on('load', e => onChange(map, e));
    map.on('moveend', e => onChange(map, e));
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
function onChange(map, e) {
    console.log("Change: " + JSON.stringify(map.getBounds()));
    var xhttp = new XMLHttpRequest();
    xhttp.onreadystatechange = function() {
      if (this.readyState == 4 && this.status == 200) {
          var result = JSON.parse(this.responseText);
          for (var i = 0; i < result.length; ++i) {
            var item = result[i];
            console.log("Creating marker " + item["lat"] + ", " + item["lng"] + " with title " + item["title"])
            L.marker([item["lat"], item["lng"]]).addTo(map).bindPopup(item["title"])
            // L.popup().setLatLng(L.latlng(item["lat"], item["lng"]))
        }
      }
    };
    xhttp.open("POST", "http://localhost:5000/events", true);
    xhttp.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
    xhttp.send(JSON.stringify(map.getBounds()))
    console.log(xhttp.responseText)
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