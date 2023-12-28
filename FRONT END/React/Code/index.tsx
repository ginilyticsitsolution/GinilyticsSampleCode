import { useContext, useEffect, useRef, useState } from "react";
import { ChatModel, ChatBubble } from "../../api/models/chatModel";
import Brending from "../Brending";
import { iconsImgs } from "../../utils/icons";
import './ChatPages.scss'
import { useLocation } from "react-router-dom";
import { localStorageKeys } from "../../utils/localStorageKeys";
import AuthenticationResponse from "../../api/responses/AuthenticationResponse";
import { getUserAvatar } from "../../utils/getMedia";

const ChatPages = () => {

  const chatContainerRef = useRef<HTMLDivElement>(null);
  const [chatModel, setChatModels] = useState<ChatModel>();
  const [messageInput, setMessageInput] = useState("");
  const initChatModels = () => {
    const { messages, offer, promoterName, currentUserId } = location.state;
    const data = ChatModel.getData(messages, offer, promoterName, currentUserId);
    setChatModels(data);
  };
  const location = useLocation();

  useEffect(() => {
    initChatModels();
  }, []);

  useEffect(() => {
    if (chatContainerRef.current) {

      const chatContainer = chatContainerRef.current;
      const isChatContainerOverflowing = chatContainer.scrollHeight > chatContainer.clientHeight;

      if (isChatContainerOverflowing) {
        chatContainer.scrollTop = chatContainer.scrollHeight;
      }
    }
  }, [chatModel?.bubbles]);

  const userInfo: AuthenticationResponse = JSON.parse(localStorage.getItem(localStorageKeys.loginResponse) as string);
  const currentUserName = `${userInfo.firstName} ${userInfo.lastName}`
  const handleSendMessage = () => {

    if (messageInput.trim() !== "") {

      if (chatModel) {
        const newBubble = new ChatBubble(currentUserName, "Just now", messageInput, "", new Date());
        const updatedBubbles = [...chatModel.bubbles, newBubble];
        const updatedChatModels = { ...chatModel, bubbles: updatedBubbles };
        setChatModels(updatedChatModels);
      }
      setMessageInput("");
    }
  };

  return (
    <>
      <Brending />

      {chatModel && chatModel.bubbles.length > 0 ? (
        <div className="centerGroup">
          <div className="desktopUI">
            <div className="containerChat p-0" ref={chatContainerRef}>
              <div className="row">
                <div className="col-auto p-2">
                  <img src={chatModel.avatar} alt="" className="chatUserImg rounded-circle m-2" />
                </div>

                <div className="col align-self-center headerText">
                  <p className="chatHeaderTitle p-0 m-0">{chatModel.headerTitle}</p>
                  <p className="chatHeaderSub yellowTxt p-0 m-0">at {chatModel.headerSub}</p>
                  <p className="chatDate p-0 m-0">Last seen at {chatModel.lastSeen}</p>
                </div>
              </div>

              {chatModel.bubbles.map((bubble, index) => {
                if (bubble.senderName === currentUserName || bubble.senderName === userInfo.firstName) {
                  return (

                    <>
                      <div className="d-flex justify-content-end me-3">
                        <div className="talkBubbleRight triRight round btmRight">
                          <div className="talktext">
                            <p className="text-white m-0">{bubble.message}</p>
                          </div>
                        </div>
                      </div>

                      <div key={index} className="chatBubbleRight">
                        <div className="row">
                          <div className="d-flex p-0 col align-items-center">
                            <div className="col seenTimeLeft d-flex p-3  align-items-center">
                              <img className="chatCheck" src={iconsImgs.ChatCheck} alt="" />
                              <p className="m-0">{bubble.timestamp}</p>
                            </div>
                            <p className="userNameBubble m-2">{currentUserName}</p>
                          </div>

                          <div className="col-auto m-2 p-0 chatUserImgBubbleRight ">
                            <img src={getUserAvatar(`${chatModel.userId}?${Date.now()}`)} alt="" className=" chatUserImgBubble rounded-circle " />
                          </div>
                        </div>
                      </div>
                    </>

                  );
                } else {
                  return (
                    <div key={index} className="chatBubbleLeft">
                      <div className="talkBubble triRight round btmLeft">
                        <div className="talktext">
                          <p className="text-white m-0">{bubble.message}</p>
                        </div>
                      </div>

                      <div className="row">
                        <div className="chatUserImgBubble col-auto mt-2 m-2">
                          <img
                            src={chatModel.avatar}
                            alt=""
                            className="chatUserImgBubble rounded-circle"
                          />
                        </div>

                        <div className="col p-0 align-self-center ms-2">
                          <div className="d-flex p-0 justify-content-start align-items-center">
                            <p className="userNameBubble m-0">{bubble.senderName}</p>
                            <div className="col d-flex p-3 justify-content-end align-items-center">
                              <p className="seenTime m-0">{bubble.timestamp}</p>
                              <img className="chatCheck" src={iconsImgs.ChatCheck} alt="" />
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  );
                }
              })}

              <div className="chatInput mb-2 row">
                <div className="cameraIcon col-auto me-2">
                  <img src={iconsImgs.ChatCamera} alt="" />
                </div>

                <div className="chatText col-auto">
                  <input
                    type="text"
                    name=""
                    id=""
                    placeholder="Your message here..."
                    value={messageInput}
                    onChange={(e) => setMessageInput(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === "Enter") {
                        e.preventDefault();
                        handleSendMessage();
                      }
                    }}
                  />
                </div>
              </div>
            </div>
          </div>
        </div>
      ) : (
        <div className="container text-center newTabContent">
          <p>
            No chats were found!
          </p>
        </div>
      )}
    </>
  );
};

export default ChatPages;
