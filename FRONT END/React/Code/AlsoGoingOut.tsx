import React, { useEffect, useState } from "react";
import AuthClient from "../../api/AuthClient";
import { parseMyPartyResponse } from "../../api/parseResponse/parseMyPartiesResponse";
import { MyPartyModel } from "../../api/models/types";
import BookingType from "../../api/enums/BookingType";
import AlsoGoingOutGroupsApi from "../../api/AlsoGoingOutGroupsApi";
import AlsoGoingOutGroupDetail from "./AlsoGoingOutGroups";
import styles from "../MyPartyDetail/myPartyDetail.module.scss";

interface Props {
  authClient: AuthClient;
}

const AlsoGoingOut: React.FC<Props> = ({ authClient }) => {

  const [alsoGoingOutGroups, setAlsoGoingOutGroups] = useState<MyPartyModel[] | undefined>();

  useEffect(() => {
    const fetchGroups = async () => {
      const myParties = new AlsoGoingOutGroupsApi(authClient);

      try {
        const response = await myParties.getAlsoGoingOutGroups();
        if (response) {
          const fetchedGroups = response.data?.map(parseMyPartyResponse);
          setAlsoGoingOutGroups(fetchedGroups);

          if (fetchedGroups?.length === 0) {
            console.log("Returned data is empty");
          }
        }
      } catch (err) {
        console.error("Error fetching groups:", err);
      }
    };

    fetchGroups();
  }, []);

  if (alsoGoingOutGroups === null) {
    return null;
  }

  if (alsoGoingOutGroups?.length === 0) {
    return (
      <div className="m-4 noGroups p-4 text-white text-center">No groups</div>
    );
  }

  function mapBookingType(bookingTypeString: string): BookingType {
    switch (bookingTypeString.toLowerCase()) {
      case 'guestlist':
        return BookingType.GuestList;
      case 'tablebooking':
        return BookingType.TableBooking;
      default:
        return BookingType.NotSet;
    }
  }

  const hasTableBooking = alsoGoingOutGroups && alsoGoingOutGroups?.some(obj => {
    const enumValue = mapBookingType(obj.party.bookingType.toString());
    return enumValue === BookingType.TableBooking;
  });

  const hasGuestList = alsoGoingOutGroups && alsoGoingOutGroups?.some(obj => {
    const enumValue = mapBookingType(obj.party.bookingType.toString());
    return enumValue === BookingType.GuestList;
  });

  const tableGroups = alsoGoingOutGroups?.filter((group) => mapBookingType(group.party.bookingType.toString()) === BookingType.TableBooking);
  const guestGroups = alsoGoingOutGroups?.filter((group) => mapBookingType(group.party.bookingType.toString()) === BookingType.GuestList);

  return (
    <>
      {hasTableBooking &&
        <div className="goingOutHeading d-flex w-100">
          <p> VIP Table Groups </p>
        </div>
      }
      {hasTableBooking && tableGroups?.map((group: MyPartyModel) => (
        <div>
          {hasTableBooking ? <AlsoGoingOutGroupDetail authClient={authClient} alsoGoingOutGroups={alsoGoingOutGroups} group={group} /> : ""}
        </div>
      ))}
      <div className={styles.line} />
      {hasGuestList &&
        <div className="goingOutHeading d-flex w-100">
          <p> GuestList </p>
        </div>
      }
      {hasGuestList && guestGroups?.map((group: MyPartyModel) => (
        <div>
          {hasGuestList ? <AlsoGoingOutGroupDetail authClient={authClient} alsoGoingOutGroups={alsoGoingOutGroups} group={group} /> : ""}
        </div>
      ))}
    </>
  );
};

export default AlsoGoingOut;
