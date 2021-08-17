using System;
using System.Collections.Generic;
using System.Text;
using ProtoBuf;

namespace NetWorkingCSharp
{
    public struct HeaderGameData
    {
        public enum EDataType
        {
            DEFAULT = 0,
            CARD,
            UNO,
            COUNTERUNO,
            CHOOSECOLOR,
            CHOOSEPLAYER
        }

        public EDataType dataType;
        public object GameData;

        public HeaderGameData(EDataType type, object gameData)
        {
            dataType = type;
            GameData = gameData;
        }

        public void SendData(System.Net.Sockets.NetworkStream stream)
        {
            Serializer.SerializeWithLengthPrefix<EDataType>(stream, dataType, PrefixStyle.Fixed32);
            switch (dataType)
            {
                case EDataType.CARD:
                    Serializer.SerializeWithLengthPrefix<UnoNetworkingGameData.GameData>(stream, (UnoNetworkingGameData.GameData)GameData,
                                                                                            PrefixStyle.Fixed32);
                    break;
                case EDataType.CHOOSECOLOR:
                    Serializer.SerializeWithLengthPrefix<PlayerGameData.CardType.Color>(stream, (PlayerGameData.CardType.Color)GameData,
                                                                                            PrefixStyle.Fixed32);
                    break;
                case EDataType.CHOOSEPLAYER:
                    Serializer.SerializeWithLengthPrefix<int>(stream, (int)GameData, PrefixStyle.Fixed32);
                    break;
            }
        }

        static public HeaderGameData ReadHeaderGameData(System.Net.Sockets.NetworkStream stream)
        {
            EDataType dataType = Serializer.DeserializeWithLengthPrefix<EDataType>(stream, PrefixStyle.Fixed32);

            object data = null;
            switch (dataType)
            {
                case EDataType.CARD:
                    data = Serializer.DeserializeWithLengthPrefix<UnoNetworkingGameData.GameData>(stream, PrefixStyle.Fixed32);
                    break;
                case EDataType.CHOOSECOLOR:
                    data = Serializer.DeserializeWithLengthPrefix<PlayerGameData.CardType.Color>(stream, PrefixStyle.Fixed32);
                    break;
                case EDataType.CHOOSEPLAYER:
                    data = Serializer.DeserializeWithLengthPrefix<int>(stream, PrefixStyle.Fixed32);
                    break;
                
            }

            return new HeaderGameData(dataType, data);
        }

    }
}