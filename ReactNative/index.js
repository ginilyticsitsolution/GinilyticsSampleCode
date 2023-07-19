import React, { useState, useRef, useEffect } from 'react';
import { DeviceEventEmitter, ToastAndroid } from "react-native"
import MapView, { PROVIDER_GOOGLE } from 'react-native-maps';
import {
  Button,
  Platform,
  SafeAreaView,
  StatusBar,
  StyleSheet,
  Text,
  View,
  DrawerLayoutAndroid,
  Image,
  Keyboard,
  KeyboardAvoidingView,
  TextInput,
  TouchableWithoutFeedback,
  TouchableOpacity
} from 'react-native';

import Geolocation from  '@react-native-community/geolocation'

import TextInputContainer from '../../components/TextInputContainer';
import SocketIOClient from 'socket.io-client';
import {
  mediaDevices,
  RTCPeerConnection,
  RTCView,
  RTCIceCandidate,
  RTCSessionDescription,
} from 'react-native-webrtc';
import CallEnd from '../../asset/CallEnd';
import CallAnswer from '../../asset/CallAnswer';
import MicOn from '../../asset/MicOn';
import MicOff from '../../asset/MicOff';
import InCallManager from 'react-native-incall-manager';
import Svg, { Path } from "react-native-svg";
import IconContainer from '../../components/IconContainer';
import IconCon from '../../components/IconCon';
import DrawerItems from './DataInfo/DrawerItems';
import TripsPage from './components/Trips';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import CallPage from './components/Calls';
const Stack = createNativeStackNavigator();
function ProfilePic() {
  return (

    <View style={[styles.alternativeLayoutButtonContainer, { marginTop: "18%" }]}>

      <IconContainer Icon={() => <Image source={require("../../asset/icon-userr.png")} borderRadius={30} />}

      />

      <Text style={{
        fontSize: 20,
        color: "white",
        marginTop: "5%",
        marginRight: "30%"
      }}>
        Jon Smith
      </Text>
    </View>

  );
}

const NavigationView = (props) => (
  <View style={[styles.navigationContainer]}>
    <View style={{ flex: 1, backgroundColor: "black" }}>
      <ProfilePic />
    </View>
    <View style={{ flex: 4, backgroundColor: "white" }}>
      <DrawerItems {...props} />
    </View>
  </View>
);


class VoIP {
  localStream
  remoteStream
  callerId
  otherUserId
  socket
  localMicOn
  peerConnection
  remoteRTCMessage
  navigationRef
  remoteCandiates
  constructor(navigationRef,callerId,otherUserId=null){
    this.remoteCandiates=[];

    this.navigationRef = navigationRef;
    this.callerId = callerId;

    this.otherUserId = otherUserId;
    
    this.localMicOn = true ;
   
    this.initSocket();

    this.InitDevices().then(
      ()=>{
        this.initWebRtc();
      }
    ).catch(e=>{

      console.log("Error Webrtc: ",e);
    });
    
}

close(){

   if(this.peerConnection.current){
      this.peerConnection.current.close();
      this.peerConnection.current = null;
   }
 
   
   if(InCallManager){
      InCallManager.stop(); 

    
  }
}
  

initSocket(){
  
  this.socket = SocketIOClient('http://192.168.10.105:3500', {
    transports: ['websocket'],
    query: {
      callerId:this.callerId,
    },
  });
  this.socket.on('newCall', data => {


    this.remoteRTCMessage.current = data.rtcMessage;
    this.otherUserId.current = data.callerId;
    
    this.navigationRef.current.reset({
      index: 1,
      routes: [
        { name: 'Home' },
        { name: 'INCOMING_CALL' }
      ],
    });
    InCallManager.startRingtone();
;
  });
  this.socket.on("leaveCall", data => {
  try{

    this.peerConnection.current.close();
  }catch(ex){

    console.log("leaveCall-Error: ",ex);
  }
    

    this.close(); 


    this.initWebRtc();
    
    this.navigationRef.current.reset({
      index: 1,
      routes: [
        { name: 'Home' },
        { name: "Call" },
      ],
    });
  });
  this.socket.on('callAnswered', async data => {
    this.remoteRTCMessage.current = data.rtcMessage;
    await this.peerConnection.current.setRemoteDescription(
      new RTCSessionDescription(this.remoteRTCMessage.current),
    );

    this.navigationRef.current.reset({
      index: 3,
      routes: [
        { name: 'Home' },
        { name: "Call" },
        { name: "JoinScreen" },
        { name: 'WEBRTC_ROOM' }

      ],
    });
  });
  
  this.socket.on('ICEcandidate', data => {
    const message = data.rtcMessage;
    
    const iceCandiate   = new RTCIceCandidate({
      candidate: message.candidate,
      sdpMid: message.id,
      sdpMLineIndex: message.label,
    });

    if (this.peerConnection.current.remoteDescription == null) {
      return this.remoteCandiates.push( iceCandiate);
    };

    if (this.peerConnection.current) {
      this.peerConnection.current
        .addIceCandidate(iceCandiate)
        .then(data => {
          console.log('SUCCESS');
        })
        .catch(err => {
          console.log('Error-onIce', err);
        });
    }
  });

}//End socket
async InitDevices() {

  try {
    let mediaConstraints = {
      audio: true,
      video:false
    };
    const mediaStream = await mediaDevices.getUserMedia( mediaConstraints );
  
    // if ( true ) {
    //   let videoTrack = mediaStream.getVideoTracks()[ 0 ];
    //   videoTrack.enabled = false;
    // };
    
    this.localStream = mediaStream;
  } catch( err ) {
    // Handle Error
    console.log("Error Media-",err);
    return;
  };



    InCallManager.start();
    InCallManager.setKeepScreenOn(true);
    InCallManager.setForceSpeakerphoneOn(true);
  

}//init Devices

initWebRtc(){

    this.peerConnection = { 
  current: new RTCPeerConnection({
      iceServers: [
        {
          urls: 'stun:stun.l.google.com:19302',
        },
        {
          urls: 'stun:stun1.l.google.com:19302',
        },
        {
          urls: 'stun:stun2.l.google.com:19302',
        },
      ],
    }),

  };
  
    this.remoteRTCMessage={
      current:null
    }

    this.peerConnection.current.addEventListener( 'connectionstatechange', event => {
      switch( this.peerConnection.current.connectionState ) {
        case 'closed':

        //this.peerConnection.current.close();
        //this.localStream.close();

          // You can handle the call being disconnected here.
    
          break;
      };
    } );

  
  this.peerConnection.current.addEventListener( 'icecandidate', event => {
	// When you find a null candidate then there are no more candidates.
	// Gathering of candidates has finished.
  
	if ( !event.candidate ) {
     console.log('End of candidates.');
     return; 
};

  
  if (event.candidate) {
    this.sendICEcandidate({
      calleeId: this.otherUserId.current,
      rtcMessage: {
        label: event.candidate.sdpMLineIndex,
        id: event.candidate.sdpMid,
        candidate: event.candidate.candidate,
      },
    });
  }
	// Send the event.candidate onto the person you're calling.
	// Keeping to Trickle ICE Standards, you should send the candidates immediately.
  } );
  
  this.peerConnection.current.addEventListener( 'negotiationneeded', event => {
    console.log("offer");
    // You can start the offer stages here.
    // Be careful as this event can be called multiple times.
  } );

  this.peerConnection.current.addEventListener( 'iceconnectionstatechange', event => {
    switch( (this.peerConnection.iceConnectionState+"").toLocaleLowerCase()) {
      case 'connected':
      case 'completed':
        // You can handle the call being connected here.
        // Like setting the video streams to visible.
        DeviceEventEmitter.emit("ice.connection.completed");
        break;
    };
  } );

  this.peerConnection.current.addEventListener( 'signalingstatechange', event => {
    switch( this.peerConnection.signalingState ) {
      case 'closed':
        // You can handle the call being disconnected here.
       
        // this.socket.off('newCall');
        // this.socket.off('callAnswered');
        // this.socket.off('ICEcandidate');

        break;
    };
  } );
  
  this.peerConnection.current.onaddstream  = event => {
    // Grab the remote track from the connected participant.
    this.remoteStream=event.stream;
 
  };
  
  this.peerConnection.current.addStream(this.localStream )
  

}



//End Init
     sendICEcandidate(data) {
      this.socket.emit('ICEcandidate', data);
     }

     async  processCall() {

      const sessionDescription = await this.peerConnection.current.createOffer();

       await this.peerConnection.current.setLocalDescription(sessionDescription);
         
      this.sendCall({
        calleeId: this.otherUserId.current,
        rtcMessage: sessionDescription,
      });
  }

     async processCandidates(){

            if(this.remoteCandiates.length<1)  {return ;};

              this.remoteCandiates.map(candidate=>{
                this.peerConnection.current
                .addIceCandidate(candidate)
                .then(data => {
                  console.log('SUCCESS');
                })
                .catch(err => {
                  console.log('Error-onIce', err);
                });
              
              });
            
            this.remoteCandiates=[];
          } 

     async  processAccept() {
        
      this.peerConnection.current.setRemoteDescription(
        new RTCSessionDescription(this.remoteRTCMessage.current),
      );
      const sessionDescription = await this.peerConnection.current.createAnswer();
      await this.peerConnection.current.setLocalDescription(sessionDescription);

      this.answerCall({
        callerId: this.otherUserId.current,
        rtcMessage: sessionDescription,
      });
      InCallManager.stopRingtone();
      this.processCandidates();
    }//End Accept Call

     answerCall(data) {
        this.socket.emit('answerCall', data);
     }

      sendCall(data) {
      this.socket.emit('call', data);
     }

  async leave() {

      this.close();
      this.initWebRtc();

      this.navigationRef.current.reset({
        index: 0,
        routes: [
          { name: 'Home' },

        ],
      });

      this.socket.emit("endCall", {
        Id: this.otherUserId.current
      });
    }
}//End class VoIp




export default function Home(props) {
 

  const base =  "https://projects.ginilytics.org:8030"; 
  const drawer = useRef(null);
  const navigationRef = useRef(null);
  const otherUserId   = useRef(null);

  const [voip,setVoip] = useState(new VoIP(navigationRef,props.userId,otherUserId));
  
  const  [watchID] = useState(Geolocation.watchPosition(
  (position) => {

      //Will give you the location on location change
        let url = base+"/CabCurrentLocation/AddCabLocation";
        console.log(url);
        console.log(position.coords);
        fetch(url,{
          method: "POST",
          headers:{
            Accept: 'application/json',
      
          },
          body:JSON.stringify({
                latitude: position.coords.latitude,
                
                longitude:position.coords.longitude,
                uniqueId: "test",
               })
        }).then(res=>{

        }).catch(ex=>{
                       console.log("error=",ex);
        });
       },
    (error) => {
      console.log("other error=",error.message);
    },
    {
      enableHighAccuracy: true,
      maximumAge: 1000
    },
  ));
  

  useEffect(() => {
      
    DeviceEventEmitter.addListener("event.logoutUser", (e) => props.logoutUser());
    DeviceEventEmitter.addListener("event.openDrawer", (e) => drawer.current.openDrawer());
    DeviceEventEmitter.addListener("event.closeDrawer", (e) => drawer.current.closeDrawer());

    DeviceEventEmitter.addListener("event.call", (e) => {

      voip.otherUserId.current = e;

      navigationRef.current.reset({
        index: 2,
        routes: [
          { name: 'Home' },
          { name: 'Call' },
          { name: 'JoinScreen' }
        ],
      });
    });



    return () => {
      DeviceEventEmitter.removeAllListeners();
    }
  },

  );

  const logoutUser = () => DeviceEventEmitter.emit("event.logoutUser");

  const JoinScreen = () => {
    return (
      <KeyboardAvoidingView
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        style={{
          flex: 1,
          backgroundColor: '#050A0E',
          justifyContent: 'center',
          paddingHorizontal: 42,
        }}>

        <TouchableWithoutFeedback onPress={Keyboard.dismiss}>
          <>
            <View
              style={{
                padding: 35,
                backgroundColor: '#1A1C22',
                justifyContent: 'center',
                alignItems: 'center',
                borderRadius: 14,
              }}>
              <Text
                style={{
                  fontSize: 18,
                  color: '#D0D4DD',
                }}>

              </Text>
              <View
                style={{
                  flexDirection: 'row',
                  marginTop: 24,
                  alignItems: 'center',
                }}>
                <Text
                  style={{
                    fontSize: 12,
                    color: '#ffff',
                    letterSpacing: 6,
                  }}>
                  {voip.callerId}
                </Text>
              </View>
            </View>

            <View
              style={{
                backgroundColor: '#1A1C22',
                padding: 40,
                marginTop: 25,
                justifyContent: 'center',
                borderRadius: 14,
              }}>
              <Text
                style={{
                  fontSize: 18,
                  color: '#D0D4DD',
                }}>
                Enter call id of another user
              </Text>
              <TextInputContainer
                placeholder={'Enter Caller ID'}
                value={voip.otherUserId.current}
                setValue={text => {
                  otherUserId.current = text;
                  }}
                keyboardType={'number-pad'}
              />
              <TouchableOpacity
                onPress={() => {
                   navigationRef.current.reset({
                    index: 3,
                    routes: [
                      { name: 'Home' },
                      { name: 'Call' },
                      { name: 'JoinScreen' },
                      { name: 'OutGoingScreen' }
                    ],
                  });

                 if(voip.peerConnection==null){
                    voip.initWebRtc();
                 }


                  voip.processCall();

                }}
                style={{
                  height: 50,
                  backgroundColor: '#5568FE',
                  justifyContent: 'center',
                  alignItems: 'center',
                  borderRadius: 12,
                  marginTop: 16,
                }}>
                <Text
                  style={{
                    fontSize: 16,
                    color: '#FFFFFF',
                  }}>
                  Call Now
                </Text>
              </TouchableOpacity>
            </View>
          </>
        </TouchableWithoutFeedback>
      </KeyboardAvoidingView>
    );
  };

  const OutgoingCallScreen = () => {
    return (
      <View
        style={{
          flex: 1,
          justifyContent: 'space-around',
          backgroundColor: '#050A0E',
        }}>
        <View
          style={{
            padding: 35,
            justifyContent: 'center',
            alignItems: 'center',
            borderRadius: 14,
          }}>
          <Text
            style={{
              fontSize: 16,
              color: '#D0D4DD',
            }}>
            Calling to...
          </Text>

          <Text
            style={{
              fontSize: 28,
              marginTop: 12,
              color: '#ffff',
              letterSpacing: 6,
            }}>
            {voip.otherUserId.current}
          </Text>
        </View>
        <View
          style={{
            justifyContent: 'center',
            alignItems: 'center',
          }}>
          <TouchableOpacity
            onPress={() => {

              navigationRef.current.reset({
                index: 2,
                routes: [
                  { name: 'Home' },
                  { name: 'Call' },
                  { name: 'JoinScreen' },
                ],
              });

              voip.otherUserId.current = null;
            }}
            style={{
              backgroundColor: '#FF5D5D',
              borderRadius: 30,
              height: 60,
              aspectRatio: 1,
              justifyContent: 'center',
              alignItems: 'center',
            }}>
            <CallEnd width={50} height={12} />
          </TouchableOpacity>
        </View>
      </View>
    );
  };

  const IncomingCallScreen = () => {
    return (
      <View
        style={{
          flex: 1,
          justifyContent: 'space-around',
          backgroundColor: '#050A0E',
        }}>
        <View
          style={{
            padding: 35,
            justifyContent: 'center',
            alignItems: 'center',
            borderRadius: 14,
          }}>
          <Text
            style={{
              fontSize: 28,
              marginTop: 12,
              color: '#ffff',
            }}>
            {voip.otherUserId.current} is calling..
          </Text>
        </View>
        <View
          style={{
            justifyContent: 'center',
            alignItems: 'center',
          }}>
          <TouchableOpacity
            onPress={() => {
         
              if(voip.peerConnection==null){
                   voip.initWebRtc();
              }

              voip.processAccept();
              
              navigationRef.current.reset({
                index: 1,
                routes: [
                  { name: 'Home' },
                  { name: 'WEBRTC_ROOM' }

                ],
              });
              }}
            style={{
              backgroundColor: 'green',
              borderRadius: 30,
              height: 60,
              aspectRatio: 1,
              justifyContent: 'center',
              alignItems: 'center',
            }}>
            <CallAnswer height={28} fill={'#fff'} />
          </TouchableOpacity>
        </View>
      </View>
    );
  };

  const WebrtcRoomScreen = () => {
    return (


      <View
        style={{
          flex: 1,
          backgroundColor: '#050A0E',
          paddingHorizontal: 12,
          paddingVertical: 12,
        }}>
        {voip.localStream ? (
          <RTCView

            style={{ backgroundColor: '#050A0E' }}
            streamURL={voip.localStream.toURL()}
          />
        ) : null}
        {voip.remoteStream ? (
          <RTCView
            style={{
              flex: 1,
              backgroundColor: '#050A0E',
              marginTop: 8,
            }}
            streamURL={voip.remoteStream.toURL()}
          />
        ) : null}
        <View
          style={{
            padding: 35,
            justifyContent: 'center',
            alignItems: 'center',
            borderRadius: 14,
            flex: 1
          }}>
          <Text
            style={{
              fontSize: 16,
              color: '#D0D4DD',
            }}>
            On Call
          </Text>

          <Text
            style={{
              fontSize: 36,
              marginTop: 12,
              color: '#ffff',
              letterSpacing: 6,
            }}>
            {voip.otherUserId.current}
          </Text>
        </View>


        <View
          style={{
            marginVertical: 12,
            flexDirection: 'row',
            justifyContent: 'space-evenly',
          }}>

          <IconContainer
            backgroundColor={'red'}
            onPress={() => {
              voip.leave();
            }}
            Icon={() => {
              return <CallEnd height={26} width={26} fill="#FFF" />;
            }}
          />
          <IconContainer
            style={{
              borderWidth: 1.5,
              borderColor: '#2B3034',
            }}
            backgroundColor={!voip.localMicOn ? '#fff' : 'transparent'}
            onPress={() => {
              //toggleMic();
            }}
            Icon={() => {
              return voip.localMicOn ? (
                <MicOn height={24} width={24} fill="#FFF" />
              ) : (
                <MicOff height={28} width={28} fill="#1D2939" />
              );
            }}
          />

        </View>
      </View>
    );
  };

  const HomeComp = () => {

    return (<DrawerLayoutAndroid
      ref={drawer}
      drawerWidth={300}
      drawerPosition='left'
      renderNavigationView={() => <NavigationView drawer={drawer}  {...props} navigation={navigationRef} logoutUser={logoutUser} />}

    >
      <NavigationContainer>
        <Stack.Navigator>
          <Stack.Screen
            name="Home"
            component={HomePage}
            options={({ navigation }) => {
              navigationRef.current = navigation;
              return {
                title: "Home"
              }
            }}
          />
          <Stack.Screen name="Trips" component={TripsPage} />
          <Stack.Screen name="Call" component={CallPage} />
          <Stack.Screen name="JoinScreen" component={JoinScreen} />
          <Stack.Screen name="OutGoingScreen" component={OutgoingCallScreen} />
          <Stack.Screen name="INCOMING_CALL" component={IncomingCallScreen} />
          <Stack.Screen name="WEBRTC_ROOM" component={WebrtcRoomScreen} />


        </Stack.Navigator>
      </NavigationContainer>
    </DrawerLayoutAndroid>);

  }

  switch ('Home') {
    case 'JOIN':
      return JoinScreen();
    case 'INCOMING_CALL':
      return IncomingCallScreen(); 
    case 'OUTGOING_CALL':
      return OutgoingCallScreen();
    case 'WEBRTC_ROOM':
      return WebrtcRoomScreen();
    case 'Home':
      return HomeComp();
    default:
      return null;
  }
}

function HomePage(props) {
   const gMapContext  = useRef(null);  
   
  return (
    <View
      style={[
        styles.container,
        {
          // Try setting `flexDirection` to `"row"`.
          flexDirection: 'column',
        },
      ]}>
      <View style={styles.alternativeLayoutButtonContainer}>
        <IconCon
          style={{
            borderWidth: 0.1,
            borderColor: '#2B3034',
          }}
          backgroundColor={!true ? '#fff' : 'transparent'}
          onPress={() => {

            DeviceEventEmitter.emit("event.openDrawer");

          }}
          Icon={() => {
            return <Menu fill="#1D2939" />

          }}
        />
        <Image source={require("../Login/logo/avita-logo.png")} style={{ width: 200, height: 60 }} />

      </View>

      <View style={{ flex: 3, backgroundColor: 'white' }}>
      <MapView
      ref={gMapContext}
      style={{ flex: 1 }}
      provider={PROVIDER_GOOGLE}
      showsUserLocation
      initialRegion={{
          latitude: 37.78825,
          longitude: -122.4324,
          latitudeDelta: 0.0922,
          longitudeDelta: 0.0421,
      }}
    />
      </View>
    </View>

  );
}
function Menu(props) {
  return <Svg
    xmlns="http://www.w3.org/2000/svg"
    width={32}
    height={32}
    viewBox="0 0 50 50"
    {...props}
  >
    <Path d="M0 9v2h50V9Zm0 15v2h50v-2Zm0 15v2h50v-2Z" />
  </Svg>
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    justifyContent: 'center',
  },
  buttonContainer: {
    margin: 20,
  },
  navigationContainer: {
    height: "100%",
    backgroundColor: '#ecf0f1',
  },
  alternativeLayoutButtonContainer: {
    margin: 10,
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
});

