using System;
using System.Net;
using System.Net.Sockets;
using Code.Internal.API;
using Code.Internal.API.Wrappers;
using TMPro;
using UnityEngine;

namespace Code.Internal.UserInterface.Pages
{
    public class DeviceInfoPage : Page
    {
        [SerializeField] private TextMeshProUGUI ipAddressText;
        [SerializeField] private TextMeshProUGUI deviceNameText;
        [SerializeField] private TextMeshProUGUI deviceIdText;
        [SerializeField] private TextMeshProUGUI userTypeText;
        
        protected override void OnOpen()
        {
            base.OnOpen();

            ipAddressText.text = GetLocalIPAddress();
            deviceNameText.text = SystemInfo.deviceModel;
            deviceIdText.text = SystemInfo.deviceUniqueIdentifier;

            var userType = "Гость";
            if (HttpClient.IsAuthorized)
            {
                userType = HttpClient.UserData.type switch
                {
                    UserType.Guest => "Гость",
                    UserType.SuperAdmin => "Суперадминистратор",
                    UserType.Admin => "Администратор",
                    UserType.Teacher => "Преподаватель",
                    UserType.Student => "Ученик",
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            
            userTypeText.text = userType;
        }

        protected override void OnClose()
        {
            base.OnClose();
        }
        
        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new System.Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}