using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace ProtokolLibraly {


    public class ProtokolMessage {

        public string GetMessage(string NMEA_MES) {


            Dictionary<string, string> PROTOCKOL_MESSAGE = new Dictionary<string, string> {

                {   "TIME",             "000000.00"     },
                {   "DATE",             "000000"        },
                {   "LATITUDE",         "0000.0000"     },  //Широта 
                {   "NS_INDICATOR",     "N"             },  //Север/Юг (N/S).
                {   "LONGITUDE",        "00000.0000"    },  //Долгота
                {   "EW_INDICATOR",     "E"             },  //Запад/Восток (E/W).
                {   "VELOCITY_KNOTS",   "000.0"         },  //Скорость  - в узлах 
                {   "UNITS_KNOTS",      "N"             },  //Единицы измерения - узлы
                {   "VELOCITY_KMPH",    "00.0"          },  //Скорость км/ч  - KMPH - kilometer per hour. 
                {   "UNITS_KMPH",       "K"             },  //Единицы измерения - км/ч
                {   "TRUE_COURSE",      "000.0"         }   //Истинный курс

            };

            //Словарь для фиксации изменения данных => если в пришедшем сообщении и действующем сообщении контроллера данные равны - 
            //- значит данные старые =Ю индекс == 0
            //Исли данные обновятся ==> 1
            Dictionary<string, int> PROTOCKOL_CHECKDATA = new Dictionary<string, int> {

                {   "TIME",             0               },
                {   "DATE",             0               },
                {   "LATITUDE",         0               },  //Широта 
                {   "NS_INDICATOR",     0               },  //Север/Юг (N/S).
                {   "LONGITUDE",        0               },  //Долгота
                {   "EW_INDICATOR",     0               },  //Запад/Восток (E/W).
                {   "VELOCITY_KNOTS",   0               },  //Скорость  - в узлах 
                {   "UNITS_KNOTS",      0               },  //Единицы измерения - узлы
                {   "VELOCITY_KMPH",    0               },  //Скорость км/ч  - KMPH - kilometer per hour. 
                {   "UNITS_KMPH",       0               },  //Единицы измерения - км/ч
                {   "TRUE_COURSE",      0               }   //Истинный курс
            
            }
            ;

            NMEAReader N = new NMEAReader();

            N.GetData(NMEA_MES, PROTOCKOL_MESSAGE);

            return "";
        }
    }

    public class NMEAReader {

        private const string СheckZDA = "ZDA";
        private const string CheckGGL = "GLL";
        private const string CheckRMC = "RMC";
        private const string CheckGGA = "GGA";


        /// <summary>
        /// Возвращает данные для сообщения по протоколу из полученного сообщения
        /// </summary>
        /// <param name="NMEA_MES"></param>
        /// <returns></returns>
        internal Dictionary<string,string> GetData(string NMEA_MES ,Dictionary<string , string > PROTOCKOL_MESSAGE) {



            //Все сообщения NMEA из информационного слова 
            List<string> ListMessage = new List<string>();
            
            ListMessage = SearchMessage(NMEA_MES);
            int count = ListMessage.Count;
            int i = 0;

            //Индефикатор
            string CheckIndef; 


            while (i<count) {


                CheckIndef = GetCode(ListMessage[i]);

                switch (CheckIndef) {

                    case СheckZDA: { } break;
                    case CheckGGL: { } break;
                    case CheckRMC: { } break;
                    case CheckGGA: { } break;

                }

            }
           
            return PROTOCKOL_MESSAGE;
        }

        /// <summary>
        /// Возвращает индефикатор сообщения NMEA для его чтения
        /// </summary>
        /// <param name="NMEA_MES"></param>
        /// <returns></returns>
        private string GetCode(string NMEA_MES) {

            string code = "";
            code = NMEA_MES.Substring(3,3);

            return code;
        }

        /// <summary>
        /// Ищет все NMEA сообщения в принятом сообщении и разбивает их на элементы массива 
        /// </summary>
        /// <param name="NMEA_MES"></param>
        /// <returns></returns>
        private List<string> SearchMessage(string NMEA_MES) {

            //Если пришло пустое сообщение 
            if (NMEA_MES == null) {
                return new List<string>() { "ERROR" };
            }

            List<string> MasMessage = new List<string>();
            string NMEAMessage = "";
            int indexStart;
            int indexEnd;

            while (true) {

                //когда окончилось сообщение 
                if (NMEA_MES == null) {
                    break;
                }
                //алгоритм поиска слов (index+3 - берем символы kc и +3 так как дожен быть меньше следующего слова )

                /*  for (int i = 0; i < (index + 3); i++) {

                    NMEAMessage += NMEA_MES[i];
                
                }*/

                indexStart = NMEA_MES.IndexOf('$');
                indexEnd =(2+ NMEA_MES.IndexOf('*'));

                //Сообщение не полное и его невозможно полностью прочитать 
                if(indexStart == -1 || indexEnd == -1) {
                    break;
                }

                NMEAMessage = NMEA_MES.Substring ( indexStart , indexEnd + 1 );
                MasMessage.Add(NMEAMessage);
                NMEAMessage = "";
                NMEA_MES = NMEA_MES.Remove(indexStart, indexEnd + 1);

            }

            //Возвращает массив сообщений 
            return MasMessage;
        }

    }

}
