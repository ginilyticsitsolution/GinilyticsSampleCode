import React, { useEffect, useState } from 'react';
import {
  StyleSheet,
  Text,
  View,
  TextInput,
  TouchableOpacity,
  Image,
} from 'react-native';
import MapViewDirections from 'react-native-maps-directions';
import MapView, { PROVIDER_GOOGLE, Marker, Callout } from 'react-native-maps';
import { PermissionsAndroid } from 'react-native';
import axios from 'axios';

const GOOGLE_MAPS_API_KEY = 'AIzaSyA56r__a3DAxWWo8VGVBQ5qIWtQpQ6j3MI';

const styles = StyleSheet.create({
  searchBox: {
    backgroundColor: 'white',
    flexDirection: 'row',
    alignItems: 'center',
    padding: 8,
  },
  searchText: {
    fontSize: 18,
    color: 'black',
    marginRight: 10,
  },
  container: {
    flex: 1,
  },

  input: {
    flex: 1,
    height: 40,
    borderColor: 'black',
    borderWidth: 1,
    borderRadius: 5,
    paddingHorizontal: 12,
  },
  inputbox: {
    flex: 0,
    borderColor: 'black',
    borderWidth: 1,
    borderRadius: 5,
    height: 40,
  },
  inputtext: {
    textAlign: 'center',
  },
  rowContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    padding: 15,
    backgroundColor: '#fff',

    paddingHorizontal: 10,
    marginTop: 5,
  },

  filterButton: {
    backgroundColor: 'red',
    paddingHorizontal: 15,
    paddingVertical: 8,
    borderRadius: 8,
  },

  filterButtonText: {
    color: '#fff',
    fontSize: 16,
  },
  locationIcon: {
    paddingHorizontal: 10,
    paddingVertical: 4,
  },

  mapContainer: {
    flex: 3,
    backgroundColor: 'white',
  },
  map: {
    flex: 1,
  },
  Timetext: {
    color: 'black',
    fontSize: 16,
    fontWeight: 'bold',
  },
  calloutContainer: {
    width: 240,
    padding: 7,
  },
  calloutText: {
    marginVertical:3,
    fontSize: 11,
    color: 'black',
    fontWeight: '700'
  },
});

function StartTrip(props) {
  const { route } = props;
  const [markers, setMarkers] = useState([]);
  const [origin, setOrigin] = useState({
    latitude: route.params.latitude,
    longitude: route.params.longitude,
  });
  const [selectedRoute, setSelectedRoute] = useState('');
  const [selectedTime, setSelectedTime] = useState('');
  const [searchText, setSearchText] = useState('');
  const trip = route.params.trip;
  

  useEffect(() => {
    handleSearch();
  }, []);

  const handleSearch = async () => {
    const newMarkers = []; 
    let i = 1;
    for (const { patientAddress, destinationAddress } of trip.tripDetails) {
      const addresses = [patientAddress, destinationAddress];
      const response = await axios.get(
        `https://maps.googleapis.com/maps/api/geocode/json?address=${addresses.join(
          '|'
        )}&key=${GOOGLE_MAPS_API_KEY}`
      );

      if (response.data.results.length >= 2) {
        const sourceResult = response.data.results[0];
        const destinationResult = response.data.results[1];
        const sourceMarker = {
          key: i,
          latitude: sourceResult.geometry.location.lat,
          longitude: sourceResult.geometry.location.lng,
          address: patientAddress,
          destination: destinationAddress,
          title: 'source',
        };

        const existingDestinationMarker = newMarkers.find(
          marker =>
            marker.latitude === destinationResult.geometry.location.lat &&
            marker.longitude === destinationResult.geometry.location.lng
        );

        if (!existingDestinationMarker) {
          const destinationMarker = {
            key: i + 1,
            address: destinationAddress,
            latitude: destinationResult.geometry.location.lat,
            longitude: destinationResult.geometry.location.lng,
            title: 'destination',
          };
          newMarkers.push(sourceMarker, destinationMarker); 
        } else {
          newMarkers.push(sourceMarker);
        }
      } else {
        console.log('Not Found');
      }
      i += 2;
    }

    setMarkers(newMarkers); 
  };


  return (
    <View style={styles.container}>
      <View style={styles.searchBox}>
        <Text style={styles.searchText}>Search</Text>
        <TextInput
          placeholder="Enter your search text"
          style={styles.input}
          onChangeText={text => setSearchText(text)}
          onSubmitEditing={handleSearch}
        />
      </View>
      <View style={styles.rowContainer}>
        <View style={styles.inputbox}>
          <TextInput
            style={styles.inputtext}
            placeholder="Enter cab route"
            value={selectedRoute}
            onChangeText={text => setSelectedRoute(text)}
          />
        </View>
        <Text style={styles.Timetext}>Time</Text>
        {/* Time Input Box */}
        <View style={styles.inputbox}>
          <TextInput
            style={styles.inputtext}
            placeholder="Enter time"
            value={selectedTime}
            onChangeText={text => setSelectedTime(text)}
          />
        </View>

        {/* Filter Button */}
        <TouchableOpacity style={styles.filterButton}>
          <Text style={styles.filterButtonText}>Filter</Text>
        </TouchableOpacity>

        {/* <View style={styles.locationIcon}>
          <Svg width={24} height={24} viewBox="0 0 24 24" fill="black">
            <Path d="M12 0C7.588 0 4 3.582 4 8c0 6.25 8 16 8 16s8-9.75 8-16c0-4.418-3.588-8-8-8zm0 11a3 3 0 1 1 0-6 3 3 0 0 1 0 6z" />
          </Svg>
        </View> */}
      </View>
      <View style={styles.mapContainer}>
        <MapView
          style={styles.map}
          provider={PROVIDER_GOOGLE}
          showsUserLocation
          initialRegion={{
            latitude: origin.latitude,
            longitude: origin.longitude,
            latitudeDelta: 0.0922,
            longitudeDelta: 0.0421,
          }}>
          {markers.map(marker => (
            <Marker
              key={marker.key}
              coordinate={{
                latitude: marker.latitude,
                longitude: marker.longitude,
              }}>
              {marker.title === 'source' ? (
                <View>
                  <Image
                    style={{ height: 36, width: 36 }}
                    source={require('../../../../asset/MapIcons/person.png')}
                  />
                  <Callout>
                    <View style={styles.calloutContainer}>
                      {/* <Text>Name: {marker.title}</Text> */}
                      <Text style={styles.calloutText}>Patient Address: {marker.address}</Text>
                      <Text style={styles.calloutText}>Destination Address: {marker.destination}</Text>

                    </View>
                  </Callout>
                </View>
              ) : (
                <View>
                  <Image
                    style={{ height: 36, width: 36 }}
                    source={require('../../../../asset/MapIcons/hospital.png')}
                  />
                  <Callout>
                    <View style={styles.calloutContainer}>
                      {/* <Text>Name: {marker.title}</Text> */}
                      <Text style={styles.calloutText}>Destination Address: {marker.address}</Text>
                    </View>
                  </Callout>
                </View>
              )}

            </Marker>
          ))}
          <MapViewDirections
            apikey={GOOGLE_MAPS_API_KEY}
            origin={origin}
            waypoints={markers}
            destination={markers[markers.length - 1]}
            strokeWidth={5}
            strokeColor="#1aa3ff"
          />


        </MapView>
      </View>
    </View>
  );
}

export default StartTrip;