import React, { useState } from 'react';
import {
  Button,
  Platform,
  SafeAreaView,
  StatusBar,
  StyleSheet,
  Text,
  View,
  Image,
  TextInput,
  FlatList,
  TouchableOpacity,
  DeviceEventEmitter,
} from 'react-native';
import Svg, { Path } from 'react-native-svg';

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: 'white',
    paddingTop: Platform.OS === 'android' ? StatusBar.currentHeight : 0,
  },
  header: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    alignItems: 'center',
    paddingVertical: 12,
    backgroundColor: '#F5F5F5',
    borderBottomWidth: 1,
    borderColor: '#E0E0E0',
  },
  headerButton: {
    paddingHorizontal: 16,
  },
  headerButtonText: {
    fontSize: 16,
    color: '#333',
    fontWeight: 'bold',
  },
  item: {
    fontSize: 18,
    fontWeight: 'bold',
    paddingVertical: 12,
    paddingHorizontal: 16,
  },
  outer: {
    borderBottomWidth: 1,
    borderColor: '#E0E0E0',
    paddingVertical: 10,
    paddingHorizontal: 16,
  },
  startBtn: {
    padding: 5,
    margin: 5,
    color: '#FFFFFF',
  },
});

const CallSvg = (props) => {
 return(
  <Svg
      xmlns="http://www.w3.org/2000/svg"
      width={24}
      height={24}
      fill="white"
      viewBox="0 0 24 24"
      {...props}>
      <Path
        stroke="#292D32"
        strokeMiterlimit={10}
        strokeWidth={1.5}
        d="M21.97 18.33c0 .36-.08.73-.25 1.09-.17.36-.39.7-.68 1.02-.49.54-1.03.93-1.64 1.18-.6.25-1.25.38-1.95.38-1.02 0-2.11-.24-3.26-.73s-2.3-1.15-3.44-1.98a28.75 28.75 0 0 1-3.28-2.8 28.414 28.414 0 0 1-2.79-3.27c-.82-1.14-1.48-2.28-1.96-3.41C2.24 8.67 2 7.58 2 6.54c0-.68.12-1.33.36-1.93.24-.61.62-1.17 1.15-1.67C4.15 2.31 4.85 2 5.59 2c.28 0 .56.06.81.18.26.12.49.3.67.56l2.32 3.27c.18.25.31.48.4.7.09.21.14.42.14.61 0 .24-.07.48-.21.71-.13.23-.32.47-.56.71l-.76.79c-.11.11-.16.24-.16.4 0 .08.01.15.03.23.03.08.06.14.08.2.18.33.49.76.93 1.28.45.52.93 1.05 1.45 1.58.54.53 1.06 1.02 1.59 1.47.52.44.95.74 1.29.92.05.02.11.05.18.08.08.03.16.04.25.04.17 0 .3-.06.41-.17l.76-.75c.25-.25.49-.44.72-.56.23-.14.46-.21.71-.21.19 0 .39.04.61.13.22.09.45.22.7.39l3.31 2.35c.26.18.44.39.55.64.1.25.16.5.16.78Z"
      />
    </Svg>
  );
};

function ContactItem(props) {
 return (
    <TouchableOpacity style={styles.outer}>
      <View style={{ flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center' }}>
        <Text style={styles.item}>{props.item.key}</Text>
        <TouchableOpacity
          style={{ backgroundColor: '#00cc33', borderRadius: 10, padding: 8 }}
          onPress={() => {
            console.log('Calling');
            DeviceEventEmitter.emit('event.call', `${props.item.key}`);
          }}>
          <CallSvg />
        </TouchableOpacity>
      </View>
    </TouchableOpacity>
  );
}

function Dialer(props) {
  const [phoneNumber, setPhoneNumber] = useState('');

  const handleNumberPress = (number) => {
    setPhoneNumber(phoneNumber + number);
  };

  const handleCallPress = () => {
    console.log('Calling:', phoneNumber);
    setPhoneNumber('');
  };

  const dialerKeys = [
    ['1','2','3'],
    ['4','5','6'],
    ['7','8','9'],
    ['*','0','<'], 
  ];

  return (
    <View style={{ paddingHorizontal: 16, paddingVertical: 20 }}>
      <TextInput
        style={{
          borderBottomWidth: 1,
          borderColor: '#E0E0E0',
          fontSize: 18,
          marginBottom: 20,
          paddingBottom: 10,
        }}
        placeholder="Enter phone number"
        value={phoneNumber}
        keyboardType="phone-pad"
        onChangeText={setPhoneNumber}
      />
      <View style={{ flexDirection: 'row', justifyContent: 'center' }}>
        <TouchableOpacity
          style={{
            backgroundColor: '#00cc33',
            borderRadius: 10,
            paddingVertical: 12,
            paddingHorizontal: 20,
            marginRight: 10,
          }}
          onPress={handleCallPress}>
          <Text style={{ color: 'white', fontWeight: 'bold' }}>Call</Text>
        </TouchableOpacity>
      </View>
      <View style={{ marginTop: 20 }}>
        {dialerKeys.map((row, rowIndex) => (
          <View key={rowIndex} style={{ flexDirection: 'row',alignSelf:'center'}}>
            {row.map((key, columnIndex) => (
              <TouchableOpacity
                key={columnIndex}
                style={{
                  width: 80,
                  height: 80,
                 marginHorizontal:5,
                 marginVertical:10,
                  justifyContent: 'center',
                  alignItems: 'center',
                  borderWidth: 1,
                  borderRadius:10,
                  borderColor: '#E0E0E0',
                }}
                onPress={() => {
                  if (key === 'x') {
                    setPhoneNumber(phoneNumber.slice(0, -1));
                  } else {
                    handleNumberPress(key);
                  }
                }}>
                <Text style={{ fontSize: 24 }}>{key}</Text>
              </TouchableOpacity>
            ))}
          </View>
        ))}
      </View>
    </View>
  );
}



export default function CallPage(props) {
  const [showDialer, setShowDialer] = useState(true);

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity
          style={styles.headerButton}
          onPress={() => setShowDialer(true)}>
          <Text style={styles.headerButtonText}>Dialer</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={styles.headerButton}
          onPress={() => setShowDialer(false)}>
          <Text style={styles.headerButtonText}>Contacts</Text>
        </TouchableOpacity>
        <TouchableOpacity
          style={styles.headerButton}>
          <Text style={styles.headerButtonText}>Recent</Text>
        </TouchableOpacity>
      </View>
      {showDialer ? <Dialer /> : (
        <FlatList
          data={[
            { key: 'Mike +1 9876-002-109' },
            { key: 'Finn +1 9776-002-109' },
          ]}
          renderItem={({ item }) => <ContactItem {...props} item={item} />}
        />
      )}
    </View>
  );
}
