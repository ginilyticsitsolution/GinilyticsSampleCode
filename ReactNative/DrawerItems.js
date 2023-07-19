import { color } from '@rneui/base';
import * as React from 'react';
import { View, Text,TouchableOpacity, DeviceEventEmitter } from "react-native";
import Svg, { Path } from "react-native-svg";
import { Colors } from 'react-native/Libraries/NewAppScreen';
import { ColorValue } from 'react-native';
const CallSvg = (props)=>{
        return  <Svg
        xmlns="http://www.w3.org/2000/svg"
        width={24}
        height={24}
        fill="none"
        viewBox="0 0 24 24"
        {...props}
      >
        <Path
          stroke="#292D32"
          strokeMiterlimit={10}
          strokeWidth={1.5}

          d="M21.97 18.33c0 .36-.08.73-.25 1.09-.17.36-.39.7-.68 1.02-.49.54-1.03.93-1.64 1.18-.6.25-1.25.38-1.95.38-1.02 0-2.11-.24-3.26-.73s-2.3-1.15-3.44-1.98a28.75 28.75 0 0 1-3.28-2.8 28.414 28.414 0 0 1-2.79-3.27c-.82-1.14-1.48-2.28-1.96-3.41C2.24 8.67 2 7.58 2 6.54c0-.68.12-1.33.36-1.93.24-.61.62-1.17 1.15-1.67C4.15 2.31 4.85 2 5.59 2c.28 0 .56.06.81.18.26.12.49.3.67.56l2.32 3.27c.18.25.31.48.4.7.09.21.14.42.14.61 0 .24-.07.48-.21.71-.13.23-.32.47-.56.71l-.76.79c-.11.11-.16.24-.16.4 0 .08.01.15.03.23.03.08.06.14.08.2.18.33.49.76.93 1.28.45.52.93 1.05 1.45 1.58.54.53 1.06 1.02 1.59 1.47.52.44.95.74 1.29.92.05.02.11.05.18.08.08.03.16.04.25.04.17 0 .3-.06.41-.17l.76-.75c.25-.25.49-.44.72-.56.23-.14.46-.21.71-.21.19 0 .39.04.61.13.22.09.45.22.7.39l3.31 2.35c.26.18.44.39.55.64.1.25.16.5.16.78Z"
        />
      </Svg>;
}

const LogoutSvg = ()=>{


        return  <Svg
        xmlns="http://www.w3.org/2000/svg"
        width={24}
        height={24}
        fill="none"
        viewBox="0 0 24 24"
     
      >
        <Path
          stroke="#323232"
          strokeLinecap="round"
          strokeLinejoin="round"
          strokeWidth={2}
          d="M21 12h-8M18 15l2.913-2.913v0a.123.123 0 0 0 0-.174v0L18 9M16 5v-.5 0A1.5 1.5 0 0 0 14.5 3H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h9.5a1.5 1.5 0 0 0 1.5-1.5v0-.5"
        />
      </Svg>
}

const  TripSvg=(props)=>{


        return (

                <Svg
                xmlns="http://www.w3.org/2000/svg"
                width={24}
                height={24}
                viewBox="0 0 24 24"
                {...props}
              >
                <Path d="M14.844 20H6.5C5.121 20 4 18.879 4 17.5S5.121 15 6.5 15h7c1.93 0 3.5-1.57 3.5-3.5S15.43 8 13.5 8H8.639a9.812 9.812 0 0 1-1.354 2H13.5c.827 0 1.5.673 1.5 1.5s-.673 1.5-1.5 1.5h-7C4.019 13 2 15.019 2 17.5S4.019 22 6.5 22h9.593a10.415 10.415 0 0 1-1.249-2zM5 2C3.346 2 2 3.346 2 5c0 3.188 3 5 3 5s3-1.813 3-5c0-1.654-1.346-3-3-3zm0 4.5a1.5 1.5 0 1 1 .001-3.001A1.5 1.5 0 0 1 5 6.5z" />
                <Path d="M19 14c-1.654 0-3 1.346-3 3 0 3.188 3 5 3 5s3-1.813 3-5c0-1.654-1.346-3-3-3zm0 4.5a1.5 1.5 0 1 1 .001-3.001A1.5 1.5 0 0 1 19 18.5z" />
              </Svg>
        );
}

const DrawItem  = (props)=>{
            
        return <TouchableOpacity {...props}>
         <View style={{  flexDirection:'row', padding:20,backgroundColor:"white"} }> 
               {props.Svg()}
              <Text style={{fontSize:18,marginLeft:30,color:"#00a3cc"}}  >{props.text}</Text>
        </View>
        </TouchableOpacity>
}

export default  function DrawerItems(props){

   const onLogout  = ()=>{

        props.logoutUser();
   }
   
   const onPressTrip = ()=>{

     props.navigation.current.navigate("Trips");   
     DeviceEventEmitter.emit("event.closeDrawer");

   }

   const  onPressCall = ()=>{

    props.navigation.current.navigate("Call");   
    DeviceEventEmitter.emit("event.closeDrawer");

  }

   return <View>
            <DrawItem text="Call"  Svg={()=><CallSvg />}  onPress={onPressCall}/> 
            <DrawItem text="Trips" Svg={()=><TripSvg  />}  onPress={onPressTrip} /> 
            <DrawItem text="Logout" Svg={()=><LogoutSvg  />} onPress={onLogout}/>
         </View>
}
