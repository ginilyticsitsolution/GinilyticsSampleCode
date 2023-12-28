import { ReactEventHandler, useContext, useState } from "react";
import { DateFormat, formatDate } from "../../utils/datePareser";
import { MyPartyModel } from "../../api/models/types";
import { AlsoGoingOutSelectedGroupContext } from "../../GlobalContext/selectedAlsoGoingOutGroupContext";
import { iconsImgs } from "../../utils/icons";
import Gender from "../../api/enums/Gender";
import { routes } from "../../routes/routes";
import { Link } from "react-router-dom";
import { getUserAvatar } from "../../utils/getMedia";
import AuthClient from "../../api/AuthClient";
import { localStorageKeys } from "../../utils/localStorageKeys";


interface Props {
    authClient: AuthClient;
    alsoGoingOutGroups: MyPartyModel[]
    group: MyPartyModel
}

const AlsoGoingOutGroupDetail: React.FC<Props> = ({ alsoGoingOutGroups, group }) => {

    const userImg = localStorage.getItem(localStorageKeys.userImg) || "";

    const { setSelectedAlsoGoingOutGroup } = useContext(AlsoGoingOutSelectedGroupContext);

    const formattedDate = (inputDateString: Date, inCludeTime: boolean): string => {
        return formatDate(new Date(inputDateString), inCludeTime, DateFormat.Short);
    };


    const handleClickInvitation = (groupInvitation: MyPartyModel) => {

        const selectedGroup = alsoGoingOutGroups?.find(
            (group) => group.party.id === groupInvitation.party.id
        );

        if (selectedGroup) {
            console.log("Found the selected group:", selectedGroup); // Debug log
            setSelectedAlsoGoingOutGroup(selectedGroup.invitations, [selectedGroup?.party], [selectedGroup?.requestedListings[0]], selectedGroup.promotersOffers);

        } else {
            console.log("No matching group found for the selected invitation"); // Debug log
        }
    };

    const handleImgError: ReactEventHandler<HTMLImageElement> = (event) => {
        const imgElement = event.target as HTMLImageElement;
        imgElement.src = userImg;
    };

    return (
        <>
            <div className="groupPromoter mb-3">
                <div className="desktopUI">
                    <div className="datePm d-flex align-items-center w-100">
                        <p> {formattedDate(group.party.partyDate, false)} </p>
                    </div>

                    <div className=" text-center row align-items-start">
                        <div className="guestListTitle greyBorder col">
                            <p className="mt-2">{group.party.bookingType}</p>
                        </div>
                        <div className="yellowTxt greyBorder align-content-center justify-content-center  d-flex col">
                            <p className="mt-3 mb-2">
                                {
                                    group.invitations.filter(
                                        (invitation) => (Gender[invitation.gender]).toString() === Gender.Female.toString()
                                    ).length
                                }
                            </p>
                            <img src={iconsImgs.Female} alt="" className="mt-3 Icons" />
                            <p className="mt-3 mb-2 ">
                                {
                                    group.invitations.filter(
                                        (invitation) => (Gender[invitation.gender]).toString() === Gender.Male.toString()
                                    ).length
                                }
                            </p>
                            <img src={iconsImgs.Male} alt="" className="mt-3 Icons" />
                        </div>
                        <div className="dateCreated greyBorder d-flex flex-column col">
                            <p>Created</p>
                            <p>
                                {formattedDate(group.party.createdAt, true)}
                            </p>
                        </div>
                    </div>
                    <div className=" text-center row  mt-2 agesGroup">
                        <p className="d-flex fw-semibold col-auto">Ages:</p>
                        <p className="d-flex yellowTxt col-auto p-0 ageP ">
                            {group.invitations.map((invitation) => invitation.age).join(", ")}
                        </p>
                    </div>
                    <div className="d-flex">
                        {group.invitations.map((invitation) => (
                            <Link to={routes.AlsoGoingOutDetailPage(group.party.id)} key={group.party.id} onClick={() => handleClickInvitation(group)} >
                                <img
                                    key={invitation.id}
                                    src={getUserAvatar(invitation?.id!)}
                                    alt="Profile Image"
                                    className="m-2 imgGuestA"
                                    onError={(event) => { handleImgError(event) }}
                                />
                            </Link>
                        ))}
                    </div>
                    <div className=" mt-2 row">
                        <p className="d-flex fw-semibold col-auto">Requested:</p>
                        <p className="d-flex col-auto yellowTxt p-0 ageP">
                            {group.requestedListings.map((listing) => listing.clubName)}
                        </p>
                    </div>
                    {group.promotersOffers.length > 0 ?
                        <div className="mt-2 row">
                            <p className="d-flex fw-semibold col-auto">Sent:</p>
                            <p className="d-flex col-auto yellowTxt p-0 ageP">
                                {group.promotersOffers.map((listing) => listing.offers.map((offer) => offer.clubName).join(", "))}
                            </p>
                        </div> : ""
                    }
                </div>
            </div>
        </>
    );
}

export default AlsoGoingOutGroupDetail;