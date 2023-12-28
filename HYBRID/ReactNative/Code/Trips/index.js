import React, { useState, useEffect } from 'react';
import { Modal, View, Text, TouchableOpacity, StyleSheet, FlatList, DeviceEventEmitter, Button } from 'react-native';
import axios from "axios";
import Config from '../../../config';

const styles = StyleSheet.create({
  container: {
    flex: 1,
    paddingTop: 22,
  },
  item: {
    padding: 10,
    fontSize: 18,
    color: 'black',
    height: 44,
  },
  outer: {
    borderWidth: 2,
    borderColor: "gray",
    borderRadius: 10,
    margin: 5,
  },
  startBtn: {
    padding: 2,
    margin: 5,
    color: "white",
    textAlign: 'center',
  },
  modalContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: 'rgba(0, 0, 0, 0.4)',
  },
  modalContent: {
    backgroundColor: 'white',
    padding: 40,
    borderRadius: 10,
    width: '85%',
    borderColor: 'black',
  },

});

function TripItem(props) {
  const trip = props.item;

  const [isModalVisible, setModalVisible] = useState(false);

  return (
    <TouchableOpacity style={styles.outer} onPress={() => setModalVisible(true)}>
      <View style={{ flex: 1, justifyContent: "space-between", flexDirection: 'column' }}>
        {/* {<Text style={styles.item}>Trip Id : {trip.key}</Text>} */}
        <Text style={styles.item}>Vehicle: {trip.vehicleInfo}</Text>
        <Text style={styles.item}>Start Time: {trip.startTime}</Text>
        <Text style={styles.item}>Distance: {trip.distance}</Text>
        <Text style={styles.item}>Duration: {trip.duration}</Text>
        <Text style={styles.item}>End Time: {trip.endTime}</Text>
        <TouchableOpacity style={{ backgroundColor: "#00cc33", margin: 2, borderRadius: 10 }} onPress={() => {
          DeviceEventEmitter.emit("event.startTime", trip); 
        }}><Text style={styles.startBtn}>Start Trip</Text></TouchableOpacity>
      </View>

      <Modal
        animationType="fade"
        transparent={true}
        visible={isModalVisible}
        onRequestClose={() => setModalVisible(false)}
      >
        <View style={styles.modalContainer}>
          <View style={styles.modalContent}>
            {trip.tripDetails.map((detail, index) => (
              <View key={index} >
                <Text style={{ color: "black",  fontWeight: 'bold', fontSize: 16 ,marginBottom:5 ,textAlign:'center'}}>Passenger Detail {index + 1}</Text>
                <Text  numberOfLines={1} style={{ color: "black" }}>1. Visit Type: {detail.appointment}</Text>
                <Text  numberOfLines={1} style={{ color: "black" }}>2. Name: {detail.patientName}</Text>
                <Text  numberOfLines={1} style={{ color: "black" }}>3. Phone Number: {detail.patientPhoneNumber}</Text>
                <Text  numberOfLines={1} style={{ color: "black" }}>4. Address: {detail.patientAddress}</Text>
                <Text  numberOfLines={1} style={{ color: "black" }}>5. Destination Address: {detail.destinationAddress}</Text>

              </View>
            ))}

            <TouchableOpacity style={{marginTop:20}}>
              <Button
          
                onPress={() => setModalVisible(false)}
                title="close"
                
              >
             
              </Button>
            </TouchableOpacity>
          </View>
        </View>
      </Modal>
    </TouchableOpacity>
  );
}

export default function TripsPage(props) {
  const [data, setData] = useState(null);

  useEffect(() => {
    fetchData();
    return () => {
      
    };
  },[]);

  const fetchData = async () => {
    try {
      const jwtToken = props.route.params.otherParam;
      axios.defaults.headers.common['Authorization'] = `Bearer ${jwtToken}`;

      const response = await axios.get(`${Config.BASE_URL}Trip/GetBookedList`);
      const tripDataArray = response.data;
     // console.log(tripDataArray)
      const tripItems = tripDataArray.map(tripData => {
        const trip = tripData.trip;
        const tripDetails = tripData.tripDetails;
      // console.log(tripDetails)
        const tripItem = {
          key: trip.tripId,
          vehicleInfo: trip.vehicleInfo,
          distance: trip.distance,
          duration: trip.duration,
          endTime: trip.endTime,
          startTime: trip.startTime,
          tripDetails: tripDetails,
        };

        return tripItem;
      });

      setData(tripItems);
    } catch (error) {
      console.error('Error fetching data:', error);
    }
  };

  return (
    <View style={styles.container}>
      <FlatList
        data={data}
        renderItem={({ item }) => <TripItem {...props} item={item} />}
        scrollEnabled={true}
      />
    </View>
  );
}
