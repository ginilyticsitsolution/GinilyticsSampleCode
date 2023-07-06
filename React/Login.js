import Facebook from '../assets/images/facebook.png'
import Google from "../assets/images/google.png";
import FacebookLogin from "react-facebook-login";
import { useState } from "react";
import { useNavigate } from 'react-router-dom';
import { FacebookAppId } from '../Api/FB-Authentication/FacebookAppId';
import axios from 'axios';

function Login({ setUserState }) {

    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');

    const navigate = useNavigate();
    const responseFacebook = (response) => { // Facebook response on successful or unsuccessful Auth.

        console.log(response);
        if (response.status !== 'unknown') {

            const responseFacebook = {
                Name: response.name,
                Email: response.email,
                ProviderName: 'Facebook',
                AccessToken: response.accessToken,
                UserId: response.userID

            }
            axios.post('https://localhost:44321/api/Login/SocialLoginRegister', responseFacebook)
                .then((result) => {
                    if (result.data.status) {

                        localStorage.setItem('token', result.data.resultData.accessToken);
                        localStorage.setItem('userName', result.data.resultData.name);
                        setUserState(true);
                        navigate('/');
                    } else {
                        alert("Login with different account!");
                    }
                });
        }
    };
    const handleLogin = async (e) => {
        e.preventDefault(); // Preventing form submission from refreshing the page
        try {
            const response = await axios.post('https://localhost:44321/api/Login/Login', {
                username,
                password
            });
            if (response.data.message == "Success") {
                localStorage.setItem('token', response.data.resultData.token); // Storing the token received from the login response.
                setUserState(true);
                navigate('/');
            }
        } catch (error) {
            // Console any errors
            console.error(error);
        }
    }

    return (
        <div className="login">
            <h1 className="loginTitle">Choose a Login Method</h1>
            <div className="wrapper">
                <div className="left">
                    <div className="loginButton google">
                        <img src={Google} alt="" className="icon" />
                        Google
                    </div>
                    <div>
                        <FacebookLogin
                            appId={FacebookAppId}
                            autoLoad={false}
                            fields={"name,email,picture"}
                            callback={responseFacebook}
                        />
                    </div>
                </div>
                <div className="center">
                    <div className="line" />
                    <div className="or">OR</div>
                </div>

                <form onSubmit={handleLogin} className='right'>
                    <div className="right">
                        <input type="text" placeholder="Username" value={username} onChange={e => setUsername(e.target.value)} />
                        <input type="password" placeholder="Password" value={password} onChange={e => setPassword(e.target.value)} />
                        <button type="submit" className="submit">Login</button>
                    </div>
                </form>
            </div>
        </div>
    );
}

export default Login;