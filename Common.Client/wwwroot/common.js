import { initializeApp } from 'https://www.gstatic.com/firebasejs/9.22.0/firebase-app.js';
import { getMessaging, getToken, onMessage   } from "https://www.gstatic.com/firebasejs/9.22.0/firebase-messaging.js";

if(window.fcmConfig && window.fcmVapidKey) {
    
    const fcmApp = initializeApp(window.fcmConfig);
    const fcmMsg = getMessaging(fcmApp);
    const fcmTokenCallback = 'FcmTokenCallback';
    const fcmOnMessage = 'FcmOnMessage';
    const fcmNotificationPermissionRemoved = 'FcmNotificationPermissionRemoved';
    let fcmToken = null;
    let authService = null;
    let lastNotificationPermission = Notification.permission;
    let postPermissionRequest = () => {
        if (Notification.permission === 'granted') {
            if (fcmToken === null) {
                getToken(fcmMsg, {vapidKey: window.fcmVapidKey}).then((token) => {
                    fcmToken = token;
                    authService.invokeMethodAsync(fcmTokenCallback, token);
                });
            } else {
                authService.invokeMethodAsync(fcmTokenCallback, fcmToken);
            }
        }
    };

    window.fcmInit = (dnObj) => {
        authService = dnObj;
        onMessage(fcmMsg, (msg) => {
            authService.invokeMethodAsync(fcmOnMessage, msg);
        });
    };

    window.fcmGetToken = () => {
        if (Notification.permission === 'granted') {
            postPermissionRequest();
        } else {
            Notification.requestPermission().then((p) => {
                postPermissionRequest();
            }).catch((e) => {
                console.log(e);
            });
        }
    };

    let pollNotificationPermission = null;
    pollNotificationPermission = () => {
        let currentNotificationPermission = Notification.permission;
        if (authService !== null && lastNotificationPermission === 'granted' && currentNotificationPermission !== 'granted') {
            lastNotificationPermission = currentNotificationPermission;
            authService.invokeMethodAsync(fcmNotificationPermissionRemoved);
        }
        setTimeout(pollNotificationPermission, 1000);
    };
    pollNotificationPermission();
}

window.writeTextToClipboard = function(text) {
    navigator.clipboard.writeText(text);
};

window.getWidth = function() {
    let pxWidth = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
    let emWidth = pxWidth / parseFloat(
        getComputedStyle(
            document.querySelector('html')
        )['font-size']
    );
    if (emWidth){
        return emWidth
    }
    return null;
};