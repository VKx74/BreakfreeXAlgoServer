using System;

namespace Algoserver.API.Helpers
{
    public class SupportAndResistanceResult
    {
        public bool ValidRes100 { get; internal set; }
        public bool ValidRes75a { get; internal set; }
        public bool ValidRes75b { get; internal set; }
        public bool ValidRes50a { get; internal set; }
        public bool ValidRes50b { get; internal set; }
        public bool ValidRes50c { get; internal set; }
        public bool ValidNeu100 { get; internal set; }
        public bool ValidNeu75a { get; internal set; }
        public bool ValidNeu75b { get; internal set; }
        public bool ValidNeu50a { get; internal set; }
        public bool ValidNeu50b { get; internal set; }
        public bool ValidNeu50c { get; internal set; }
        public bool ValidSup100 { get; internal set; }
        public bool ValidSup75a { get; internal set; }
        public bool ValidSup75b { get; internal set; }
        public bool ValidSup50a { get; internal set; }
        public bool ValidSup50b { get; internal set; }
        public bool ValidSup50c { get; internal set; }
    }
    public class SupportAndResistance
    {
        private decimal EightEight;
        private decimal EightEight1;
        private decimal EightEight2;
        private decimal EightEight3;
        private decimal FourEight;
        private decimal FourEight1;
        private decimal FourEight2;
        private decimal FourEight3;
        private decimal ZeroEight;
        private decimal ZeroEight1;
        private decimal ZeroEight2;
        private decimal ZeroEight3;
        private decimal Increment;
        private decimal AbsTop;
        private decimal SymbolMinTick;

        public SupportAndResistance(Levels levels, decimal SymbolMinTick)
        {
            SymbolMinTick = SymbolMinTick; Increment = levels.Level128.Increment; AbsTop = levels.Level128.AbsTop;
            EightEight = levels.Level128.EightEight; FourEight = levels.Level128.FourEight; ZeroEight = levels.Level128.ZeroEight;
            EightEight1 = levels.Level32.EightEight; FourEight1 = levels.Level32.FourEight; ZeroEight1 = levels.Level32.ZeroEight;
            EightEight2 = levels.Level16.EightEight; FourEight2 = levels.Level16.FourEight; ZeroEight2 = levels.Level16.ZeroEight;
            EightEight3 = levels.Level8.EightEight; FourEight3 = levels.Level8.FourEight; ZeroEight3 = levels.Level8.ZeroEight;
        }

        public SupportAndResistanceResult Calculate()
        {
            //rc 100s
            var rc1 = r(EightEight, EightEight1) && r(EightEight2, EightEight3) && r(EightEight3, EightEight);
            var rc2 = r(EightEight, EightEight1) && r(EightEight2, FourEight3) && r(FourEight3, EightEight);
            var rc3 = r(EightEight, EightEight1) && r(EightEight2, ZeroEight3) && r(ZeroEight3, EightEight);
            var rc4 = r(EightEight, EightEight1) && r(FourEight2, EightEight3) && r(EightEight3, EightEight);
            var rc5 = r(EightEight, EightEight1) && r(FourEight2, FourEight3) && r(FourEight3, EightEight);
            var rc6 = r(EightEight, EightEight1) && r(FourEight2, ZeroEight3) && r(ZeroEight3, EightEight);
            var rc7 = r(EightEight, EightEight1) && r(ZeroEight2, EightEight3) && r(EightEight3, EightEight);
            var rc8 = r(EightEight, EightEight1) && r(ZeroEight2, FourEight3) && r(FourEight3, EightEight);
            var rc9 = r(EightEight, EightEight1) && r(ZeroEight2, ZeroEight3) && r(ZeroEight3, EightEight);
            var rc10 = r(EightEight, FourEight1) && r(EightEight2, EightEight3) && r(EightEight3, EightEight);
            var rc11 = r(EightEight, FourEight1) && r(EightEight2, FourEight3) && r(FourEight3, EightEight);
            var rc12 = r(EightEight, FourEight1) && r(EightEight2, ZeroEight3) && r(ZeroEight3, EightEight);
            var rc13 = r(EightEight, FourEight1) && r(FourEight2, EightEight3) && r(EightEight3, EightEight);
            var rc14 = r(EightEight, FourEight1) && r(FourEight2, FourEight3) && r(FourEight3, EightEight);
            var rc15 = r(EightEight, FourEight1) && r(FourEight2, ZeroEight3) && r(ZeroEight3, EightEight);
            var rc16 = r(EightEight, FourEight1) && r(ZeroEight2, EightEight3) && r(EightEight3, EightEight);
            var rc17 = r(EightEight, FourEight1) && r(ZeroEight2, FourEight3) && r(FourEight3, EightEight);
            var rc18 = r(EightEight, FourEight1) && r(ZeroEight2, ZeroEight3) && r(ZeroEight3, EightEight);
            var rc19 = r(EightEight, ZeroEight1) && r(EightEight2, EightEight3) && r(EightEight3, EightEight);
            var rc20 = r(EightEight, ZeroEight1) && r(EightEight2, FourEight3) && r(FourEight3, EightEight);
            var rc21 = r(EightEight, ZeroEight1) && r(EightEight2, ZeroEight3) && r(ZeroEight3, EightEight);
            var rc22 = r(EightEight, ZeroEight1) && r(FourEight2, EightEight3) && r(EightEight3, EightEight);
            var rc23 = r(EightEight, ZeroEight1) && r(FourEight2, FourEight3) && r(FourEight3, EightEight);
            var rc24 = r(EightEight, ZeroEight1) && r(FourEight2, ZeroEight3) && r(ZeroEight3, EightEight);
            var rc25 = r(EightEight, ZeroEight1) && r(ZeroEight2, EightEight3) && r(EightEight3, EightEight);
            var rc26 = r(EightEight, ZeroEight1) && r(ZeroEight2, FourEight3) && r(FourEight3, EightEight);
            var rc27 = r(EightEight, ZeroEight1) && r(ZeroEight2, ZeroEight3) && r(ZeroEight3, EightEight);


            //RC 75's 

            var rcsf1 = r(EightEight, EightEight1) && r(EightEight2, EightEight) && !r(EightEight3, EightEight) && !r(FourEight3, EightEight);  //red red1 red2 !red3 !blue3
            var rcsf2 = r(EightEight, EightEight1) && r(FourEight2, EightEight) && !r(EightEight3, EightEight) && !r(FourEight3, EightEight);       //red red1 blue2 !red3 !blue3 
            var rcsf3 = r(EightEight, EightEight1) && r(ZeroEight2, EightEight) && !r(EightEight3, EightEight) && !r(FourEight3, EightEight);       //red red1 green2 !red3 !blue3 
            var rcsf4 = r(EightEight, FourEight1) && r(EightEight2, EightEight) && !r(EightEight3, EightEight) && !r(FourEight3, EightEight);       //red blue1 red2 !red3 !blue3 
            var rcsf5 = r(EightEight, FourEight1) && r(FourEight2, EightEight) && !r(EightEight3, EightEight) && !r(FourEight3, EightEight);        //red blue1 blue2 !red3 !blue3 
            var rcsf6 = r(EightEight, FourEight1) && r(ZeroEight2, EightEight) && !r(EightEight3, EightEight) && !r(FourEight3, EightEight);    //red blue1 green2 !red3 !blue3 
            var rcsf7 = r(EightEight, ZeroEight1) && r(EightEight2, EightEight) && !r(EightEight3, EightEight) && !r(FourEight3, EightEight);       //red green1 red2 !red3 !blue3 
            var rcsf8 = r(EightEight, ZeroEight1) && r(FourEight2, EightEight) && !r(EightEight3, EightEight) && !r(FourEight3, EightEight);        //red green1 blue2 !red3 !blue3 
            var rcsf9 = r(EightEight, ZeroEight1) && r(ZeroEight2, EightEight) && !r(EightEight3, EightEight) && !r(FourEight3, EightEight);        //red green1 green2 !red3 !blue3 
            var rcsf10 = r(EightEight1, EightEight2) && r(EightEight3, EightEight1) && !r(EightEight, EightEight1) && !r(FourEight, EightEight1);       //red1 red2 red3 !red !blue 
            var rcsf11 = r(EightEight1, EightEight2) && r(FourEight3, EightEight1) && !r(EightEight, EightEight1) && !r(FourEight, EightEight1);    //red1 red2 blue3 !red !blue  
            var rcsf12 = r(EightEight1, EightEight2) && r(ZeroEight3, EightEight1) && !r(EightEight, EightEight1) && !r(FourEight, EightEight1);        //red1 red2 green3 !red !blue  
            var rcsf13 = r(EightEight1, FourEight2) && r(EightEight3, EightEight1) && !r(EightEight, EightEight1) && !r(FourEight, EightEight1);        //red1 blue2 red3 !red !blue  
            var rcsf14 = r(EightEight1, FourEight2) && r(FourEight3, EightEight1) && !r(EightEight, EightEight1) && !r(FourEight, EightEight1);         //red1 blue2 blue3 !red !blue  
            var rcsf15 = r(EightEight1, FourEight2) && r(ZeroEight3, EightEight1) && !r(EightEight, EightEight1) && !r(FourEight, EightEight1);     //red1 blue2 green3 !red !blue  
            var rcsf16 = r(EightEight1, ZeroEight2) && r(EightEight3, EightEight1) && !r(EightEight, EightEight1) && !r(FourEight, EightEight1);        //red1 green2 red3 !red !blue  
            var rcsf17 = r(EightEight1, ZeroEight2) && r(FourEight3, EightEight1) && !r(EightEight, EightEight1) && !r(FourEight, EightEight1);     //red1 green2 blue3 !red !blue  
            var rcsf18 = r(EightEight1, ZeroEight2) && r(ZeroEight3, EightEight1) && !r(EightEight, EightEight1) && !r(FourEight, EightEight1);     //red1 green2 green3 !red !blue

            //rc50s)
            var rc501 = r(EightEight, EightEight1) && !r(EightEight, EightEight2) && !r(EightEight, EightEight3) && !r(EightEight, FourEight2) && !r(EightEight, FourEight3);      //red red1 !red2 !red3 !blue2 !blue3
            var rc502 = r(EightEight, ZeroEight1) && !r(EightEight, EightEight2) && !r(EightEight, EightEight3) && !r(EightEight, FourEight2) && !r(EightEight, FourEight3);       //red green1 !red2 !red3 !blue2 !blue3
            var rc503 = r(EightEight, FourEight1) && !r(EightEight, EightEight2) && !r(EightEight, EightEight3) && !r(EightEight, FourEight2) && !r(EightEight, FourEight3);       //red blue1 !red2 !red3 !blue2 !blue3
            var rc504 = r(EightEight1, EightEight2) && !r(EightEight1, EightEight) && !r(EightEight1, EightEight3) && !r(EightEight1, FourEight) && !r(EightEight1, FourEight3);    //red1 red2 !red !red3 !blue !blue3
            var rc505 = r(EightEight1, ZeroEight2) && !r(EightEight1, EightEight) && !r(EightEight1, EightEight3) && !r(EightEight1, FourEight) && !r(EightEight1, FourEight3);    //red1 green2 !red !red3 !blue !blue3
            var rc506 = r(EightEight1, FourEight2) && !r(EightEight1, EightEight) && !r(EightEight1, EightEight3) && !r(EightEight1, FourEight) && !r(EightEight1, FourEight3);    //red1 blue2 !red !red3 !blue !blue3 
            var rc507 = r(EightEight2, EightEight3) && !r(EightEight2, EightEight) && !r(EightEight2, EightEight1) && !r(EightEight2, FourEight) && !r(EightEight2, FourEight1);   //red2 red3 !red !red1 !blue !blue1
            var rc508 = r(EightEight2, ZeroEight3) && !r(EightEight2, EightEight) && !r(EightEight2, EightEight1) && !r(EightEight2, FourEight) && !r(EightEight2, FourEight1);    //red2 green3 !red !red1 !blue !blue1
            var rc509 = r(EightEight2, FourEight3) && !r(EightEight2, EightEight) && !r(EightEight2, EightEight1) && !r(EightEight2, FourEight) && !r(EightEight2, FourEight1);    //red2 blue3 !red !red1 !blue !blue1 


            //rc25s
            var ValidRes25a = !r(EightEight, EightEight1) && !r(EightEight, EightEight2) && !r(EightEight, EightEight3) && !r(EightEight, FourEight) && !r(EightEight, FourEight1) && !r(EightEight, FourEight2) && !r(EightEight, FourEight3);       // !red1 !red2 !red3 !blue !blue1 !blue2 !blue3
            var ValidRes25b = !r(EightEight1, EightEight) && !r(EightEight1, EightEight2) && !r(EightEight1, EightEight3) && !r(EightEight1, FourEight) && !r(EightEight1, FourEight1) && !r(EightEight1, FourEight2) && !r(EightEight1, FourEight3); // !red  !red2 !red3 !blue !blue1 !blue2 !blue3
            var ValidRes25c = !r(EightEight2, EightEight) && !r(EightEight2, EightEight1) && !r(EightEight2, EightEight3) && !r(EightEight2, FourEight) && !r(EightEight2, FourEight1) && !r(EightEight2, FourEight2) && !r(EightEight2, FourEight3); // !red  !red1 !red3 !blue !blue1 !blue2 !blue3
            var ValidRes25d = !r(EightEight3, EightEight) && !r(EightEight3, EightEight1) && !r(EightEight3, EightEight2) && !r(EightEight3, FourEight) && !r(EightEight3, FourEight1) && !r(EightEight3, FourEight2) && !r(EightEight3, FourEight3); // !red  !red1 !red2 !blue !blue1 !blue2 !blue3


            //NC 100
            var nc1 = r(FourEight, EightEight1) && r(EightEight2, EightEight3) && r(EightEight3, FourEight);            //blue red1 red2 red3
            var nc2 = r(FourEight, EightEight1) && r(EightEight2, FourEight3) && r(FourEight3, FourEight);              //blue red1 red2 blue3 
            var nc3 = r(FourEight, EightEight1) && r(EightEight2, ZeroEight3) && r(ZeroEight3, FourEight);          //blue red1 red2 green3	
            var nc4 = r(FourEight, EightEight1) && r(FourEight2, EightEight3) && r(EightEight3, FourEight);         //blue red1 blue2 red3 
            var nc5 = r(FourEight, EightEight1) && r(FourEight2, FourEight3) && r(FourEight3, FourEight);           //blue red1 blue2 blue3 		
            var nc6 = r(FourEight, EightEight1) && r(FourEight2, ZeroEight3) && r(ZeroEight3, FourEight);               //blue red1 blue2 green3 
            var nc7 = r(FourEight, EightEight1) && r(ZeroEight2, EightEight3) && r(EightEight3, FourEight);         //blue red1 green2 red3		
            var nc8 = r(FourEight, EightEight1) && r(ZeroEight2, FourEight3) && r(FourEight3, FourEight);               //blue red1 green2 blue3 
            var nc9 = r(FourEight, EightEight1) && r(ZeroEight2, ZeroEight3) && r(ZeroEight3, FourEight);               //blue red1 green2 green3 
            var nc10 = r(FourEight, FourEight1) && r(EightEight2, EightEight3) && r(EightEight3, FourEight);            //blue blue1 red2 red3
            var nc11 = r(FourEight, FourEight1) && r(EightEight2, FourEight3) && r(FourEight3, FourEight);              //blue blue1 red2 blue3 	
            var nc12 = r(FourEight, FourEight1) && r(EightEight2, ZeroEight3) && r(ZeroEight3, FourEight);          //blue blue1 red2 green3 
            var nc13 = r(FourEight, FourEight1) && r(FourEight2, EightEight3) && r(EightEight3, FourEight);         //blue blue1 blue2 red3 	
            var nc14 = r(FourEight, FourEight1) && r(FourEight2, FourEight3) && r(FourEight3, FourEight);               //blue blue1 blue2 blue3
            var nc15 = r(FourEight, FourEight1) && r(FourEight2, ZeroEight3) && r(ZeroEight3, FourEight);               //blue blue1 blue2 green3
            var nc16 = r(FourEight, FourEight1) && r(ZeroEight2, EightEight3) && r(EightEight3, FourEight);         //blue blue1 green2 red3 
            var nc17 = r(FourEight, FourEight1) && r(ZeroEight2, FourEight3) && r(FourEight3, FourEight);           //blue blue1 green2 blue3 
            var nc18 = r(FourEight, FourEight1) && r(ZeroEight2, ZeroEight3) && r(ZeroEight3, FourEight);               //blue blue1 green2 green3 
            var nc19 = r(FourEight, ZeroEight1) && r(EightEight2, EightEight3) && r(EightEight3, FourEight);            //blue green1 red2 red3 
            var nc20 = r(FourEight, ZeroEight1) && r(EightEight2, FourEight3) && r(FourEight3, FourEight);          //blue green1 red2 blue3 
            var nc21 = r(FourEight, ZeroEight1) && r(EightEight2, ZeroEight3) && r(ZeroEight3, FourEight);          //blue green1 red2 green3 
            var nc22 = r(FourEight, ZeroEight1) && r(FourEight2, EightEight3) && r(EightEight3, FourEight);         //blue green1 blue2 red3	
            var nc23 = r(FourEight, ZeroEight1) && r(FourEight2, FourEight3) && r(FourEight3, FourEight);           //blue green1 blue2 blue3 
            var nc24 = r(FourEight, ZeroEight1) && r(FourEight2, ZeroEight3) && r(ZeroEight3, FourEight);           //blue green1 blue2 green3 	
            var nc25 = r(FourEight, ZeroEight1) && r(ZeroEight2, EightEight3) && r(EightEight3, FourEight);         //blue green1 green2 red3 	
            var nc26 = r(FourEight, ZeroEight1) && r(ZeroEight2, FourEight3) && r(FourEight3, FourEight);           //blue green1 green2 blue3 
            var nc27 = r(FourEight, ZeroEight1) && r(ZeroEight2, ZeroEight3) && r(ZeroEight3, FourEight);           //blue green1 green2 green3

            //NCs 75's
            var ncsf1 = r(FourEight, EightEight1) && r(EightEight2, FourEight) && !r(ZeroEight3, FourEight) && !r(EightEight3, FourEight) && !r(FourEight3, FourEight);    //blue red1 red2 !green3 !red3 !blue3
            var ncsf2 = r(FourEight, EightEight1) && r(FourEight2, FourEight) && !r(ZeroEight3, FourEight) && !r(EightEight3, FourEight) && !r(FourEight3, FourEight);  //blue red1 blue2  !green3 !red3 !blue3
            var ncsf3 = r(FourEight, EightEight1) && r(ZeroEight2, FourEight) && !r(ZeroEight3, FourEight) && !r(EightEight3, FourEight) && !r(FourEight3, FourEight);  //blue red1 green2  !green3 !red3 !blue3
            var ncsf4 = r(FourEight, FourEight1) && r(EightEight2, FourEight) && !r(ZeroEight3, FourEight) && !r(EightEight3, FourEight) && !r(FourEight3, FourEight);  //blue blue1 red2  !green3 !red3 !blue3
            var ncsf5 = r(FourEight, FourEight1) && r(FourEight2, FourEight) && !r(ZeroEight3, FourEight) && !r(EightEight3, FourEight) && !r(FourEight3, FourEight);       //blue blue1 blue2  !green3 !red3 !blue3
            var ncsf6 = r(FourEight, FourEight1) && r(ZeroEight2, FourEight) && !r(ZeroEight3, FourEight) && !r(EightEight3, FourEight) && !r(FourEight3, FourEight);       //blue blue1 green2  !green3 !red3 !blue3
            var ncsf7 = r(FourEight, ZeroEight1) && r(EightEight2, FourEight) && !r(ZeroEight3, FourEight) && !r(EightEight3, FourEight) && !r(FourEight3, FourEight);  //blue green1 red2  !green3 !red3 !blue3
            var ncsf8 = r(FourEight, ZeroEight1) && r(FourEight2, FourEight) && !r(ZeroEight3, FourEight) && !r(EightEight3, FourEight) && !r(FourEight3, FourEight);       //blue green1 blue2  !green3 !red3 !blue3
            var ncsf9 = r(FourEight, ZeroEight1) && r(ZeroEight2, FourEight) && !r(ZeroEight3, FourEight) && !r(EightEight3, FourEight) && !r(FourEight3, FourEight);       //blue green1 green2  !green3 !red3 !blue3
            var ncsf10 = r(FourEight1, EightEight2) && r(EightEight3, FourEight1) && !r(ZeroEight, FourEight1) && !r(EightEight, FourEight1) && !r(FourEight, FourEight1);  //blue1 red2 red3  !green !red !blue
            var ncsf11 = r(FourEight1, EightEight2) && r(FourEight3, FourEight1) && !r(ZeroEight, FourEight1) && !r(EightEight, FourEight1) && !r(FourEight, FourEight1);   //blue1 red2 blue3 !green !red !blue
            var ncsf12 = r(FourEight1, EightEight2) && r(ZeroEight3, FourEight1) && !r(ZeroEight, FourEight1) && !r(EightEight, FourEight1) && !r(FourEight, FourEight1);   //blue1 red2 green3 !green !red !blue
            var ncsf13 = r(FourEight1, FourEight2) && r(EightEight3, FourEight1) && !r(ZeroEight, FourEight1) && !r(EightEight, FourEight1) && !r(FourEight, FourEight1);   //blue1 blue2 red3 !green !red !blue
            var ncsf14 = r(FourEight1, FourEight2) && r(FourEight3, FourEight1) && !r(ZeroEight, FourEight1) && !r(EightEight, FourEight1) && !r(FourEight, FourEight1);    //blue1 blue2 blue3 !green !red !blue
            var ncsf15 = r(FourEight1, FourEight2) && r(ZeroEight3, FourEight1) && !r(ZeroEight, FourEight1) && !r(EightEight, FourEight1) && !r(FourEight, FourEight1);    //blue1 blue2 green3 !green !red !blue
            var ncsf16 = r(FourEight1, ZeroEight2) && r(EightEight3, FourEight1) && !r(ZeroEight, FourEight1) && !r(EightEight, FourEight1) && !r(FourEight, FourEight1);   //blue1 green2 red3 !green !red !blue
            var ncsf17 = r(FourEight1, ZeroEight2) && r(FourEight3, FourEight1) && !r(ZeroEight, FourEight1) && !r(EightEight, FourEight1) && !r(FourEight, FourEight1);    //blue1 green2 blue3 !green !red !blue
            var ncsf18 = r(FourEight1, ZeroEight2) && r(ZeroEight3, FourEight1) && !r(ZeroEight, FourEight1) && !r(EightEight, FourEight1) && !r(FourEight, FourEight1);    //blue1 green2 green3 !green !red !blue

            //ncsfs (nc50s)
            var nc501 = r(FourEight, EightEight1) && !r(FourEight, ZeroEight2) && !r(FourEight, EightEight2) && !r(FourEight, FourEight2) && !r(FourEight, ZeroEight3) && !r(FourEight, EightEight3) && !r(FourEight, FourEight3);      //blue red1 !green2 !red2 !blue2 !green3 !red3 !blue3
            var nc502 = r(FourEight, ZeroEight1) && !r(FourEight, ZeroEight2) && !r(FourEight, EightEight2) && !r(FourEight, FourEight2) && !r(FourEight, ZeroEight3) && !r(FourEight, EightEight3) && !r(FourEight, FourEight3);        //blue green1 !green2 !red2 !blue2 !green3 !red3 !blue3
            var nc503 = r(FourEight, FourEight1) && !r(FourEight, ZeroEight2) && !r(FourEight, EightEight2) && !r(FourEight, FourEight2) && !r(FourEight, ZeroEight3) && !r(FourEight, EightEight3) && !r(FourEight, FourEight3);        //blue blue1 !green2 !red2 !blue2 !green3 !red3 !blue3
            var nc504 = r(FourEight1, EightEight2) && !r(FourEight1, ZeroEight) && !r(FourEight1, EightEight) && !r(FourEight1, FourEight) && !r(FourEight1, ZeroEight3) && !r(FourEight1, EightEight3) && !r(FourEight1, FourEight3);   //blue1 red2 !green !red !blue !green3 !red3 !blue3
            var nc505 = r(FourEight1, ZeroEight2) && !r(FourEight1, ZeroEight) && !r(FourEight1, EightEight) && !r(FourEight1, FourEight) && !r(FourEight1, ZeroEight3) && !r(FourEight1, EightEight3) && !r(FourEight1, FourEight3);    //blue1 green2 !green !red !blue !green3 !red3 !blue3
            var nc506 = r(FourEight1, FourEight2) && !r(FourEight1, ZeroEight) && !r(FourEight1, EightEight) && !r(FourEight1, FourEight) && !r(FourEight1, ZeroEight3) && !r(FourEight1, EightEight3) && !r(FourEight1, FourEight3);    //blue1 blue2 !green !red !blue !green3 !red3 !blue3
            var nc507 = r(FourEight2, EightEight3) && !r(FourEight2, ZeroEight) && !r(FourEight2, EightEight) && !r(FourEight2, FourEight) && !r(FourEight2, ZeroEight1) && !r(FourEight2, EightEight1) && !r(FourEight2, FourEight1);   //blue2 red3 !green !red !blue !green1 !red1 !blue1
            var nc508 = r(FourEight2, ZeroEight3) && !r(FourEight2, ZeroEight) && !r(FourEight2, EightEight) && !r(FourEight2, FourEight) && !r(FourEight2, ZeroEight1) && !r(FourEight2, EightEight1) && !r(FourEight2, FourEight1);    //blue2 red3 !green !red !blue !green1 !red1 !blue1
            var nc509 = r(FourEight2, FourEight3) && !r(FourEight2, ZeroEight) && !r(FourEight2, EightEight) && !r(FourEight2, FourEight) && !r(FourEight2, ZeroEight1) && !r(FourEight2, EightEight1) && !r(FourEight2, FourEight1);    //blue2 red3 !green !red !blue !green1 !red1 !blue1

            //nc25s
            var ValidNeu25a = !r(FourEight, FourEight1) && !r(FourEight, FourEight2) && !r(FourEight, FourEight3) && !r(FourEight, ZeroEight) && !r(FourEight, ZeroEight1) && !r(FourEight, ZeroEight2) && !r(FourEight, ZeroEight3) && !r(FourEight, EightEight) && !r(FourEight, EightEight1) && !r(FourEight, EightEight2) && !r(FourEight, EightEight3);  // !blue1 !blue2 !blue3 !green !green1 !green2 !green3 !red !red1 !red2 !red3
            var ValidNeu25b = !r(FourEight1, FourEight) && !r(FourEight1, FourEight2) && !r(FourEight1, FourEight3) && !r(FourEight1, ZeroEight) && !r(FourEight1, ZeroEight1) && !r(FourEight1, ZeroEight2) && !r(FourEight1, ZeroEight3) && !r(FourEight1, EightEight) && !r(FourEight1, EightEight1) && !r(FourEight1, EightEight2) && !r(FourEight1, EightEight3); // !blue  !blue2 !blue3 !green !green1 !green2 !green3 !red !red1 !red2 !red3
            var ValidNeu25c = !r(FourEight2, FourEight) && !r(FourEight2, FourEight1) && !r(FourEight2, FourEight3) && !r(FourEight2, ZeroEight) && !r(FourEight2, ZeroEight1) && !r(FourEight2, ZeroEight2) && !r(FourEight2, ZeroEight3) && !r(FourEight2, EightEight) && !r(FourEight2, EightEight1) && !r(FourEight2, EightEight2) && !r(FourEight2, EightEight3); // !blue  !blue1 !blue3 !green !green1 !green2 !green3 !red !red1 !red2 !red3
            var ValidNeu25d = !r(FourEight3, FourEight) && !r(FourEight3, FourEight1) && !r(FourEight3, FourEight2) && !r(FourEight3, ZeroEight) && !r(FourEight3, ZeroEight1) && !r(FourEight3, ZeroEight2) && !r(FourEight3, ZeroEight3) && !r(FourEight3, EightEight) && !r(FourEight3, EightEight1) && !r(FourEight3, EightEight2) && !r(FourEight3, EightEight3); // !blue  !blue1 !blue2 !green !green1 !green2 !green3 !red !red1 !red2 !red3


            //SC 100's
            var sc1 = r(ZeroEight, EightEight1) && r(EightEight2, EightEight3) && r(EightEight3, ZeroEight);        //green red1 red2 red3 
            var sc2 = r(ZeroEight, EightEight1) && r(EightEight2, FourEight3) && r(FourEight3, ZeroEight);          //green red1 red2 blue3 
            var sc3 = r(ZeroEight, EightEight1) && r(EightEight2, ZeroEight3) && r(ZeroEight3, ZeroEight);          //green red1 red2 green3 
            var sc4 = r(ZeroEight, EightEight1) && r(FourEight2, EightEight3) && r(EightEight3, ZeroEight);     //green red1 blue2 red3 
            var sc5 = r(ZeroEight, EightEight1) && r(FourEight2, FourEight3) && r(FourEight3, ZeroEight);           //green red1 blue2 blue3 
            var sc6 = r(ZeroEight, EightEight1) && r(FourEight2, ZeroEight3) && r(ZeroEight3, ZeroEight);           //green red1 blue2 green3 
            var sc7 = r(ZeroEight, EightEight1) && r(ZeroEight2, EightEight3) && r(EightEight3, ZeroEight);     //green red1 green2 red3
            var sc8 = r(ZeroEight, EightEight1) && r(ZeroEight2, FourEight3) && r(FourEight3, ZeroEight);           //green red1 green2 blue3 
            var sc9 = r(ZeroEight, EightEight1) && r(ZeroEight2, ZeroEight3) && r(ZeroEight3, ZeroEight);       //green red1 green2 green3 
            var sc10 = r(ZeroEight, FourEight1) && r(EightEight2, EightEight3) && r(EightEight3, ZeroEight);        //green blue1 red2 red3 
            var sc11 = r(ZeroEight, FourEight1) && r(EightEight2, FourEight3) && r(FourEight3, ZeroEight);          //green blue1 red2 blue3 
            var sc12 = r(ZeroEight, FourEight1) && r(EightEight2, ZeroEight3) && r(ZeroEight3, ZeroEight);      //green blue1 red2 green3 
            var sc13 = r(ZeroEight, FourEight1) && r(FourEight2, EightEight3) && r(EightEight3, ZeroEight);     //green blue1 blue2 red3  
            var sc14 = r(ZeroEight, FourEight1) && r(FourEight2, FourEight3) && r(FourEight3, ZeroEight);       //green blue1 blue2 blue3
            var sc15 = r(ZeroEight, FourEight1) && r(FourEight2, ZeroEight3) && r(ZeroEight3, ZeroEight);       //green blue1 blue2 green3 
            var sc16 = r(ZeroEight, FourEight1) && r(ZeroEight2, EightEight3) && r(EightEight3, ZeroEight);     //reen blue1 green2 red3 
            var sc17 = r(ZeroEight, FourEight1) && r(ZeroEight2, FourEight3) && r(FourEight3, ZeroEight);           //green blue1 green2 blue3 
            var sc18 = r(ZeroEight, FourEight1) && r(ZeroEight2, ZeroEight3) && r(ZeroEight3, ZeroEight);       //green blue1 green2 green3 
            var sc19 = r(ZeroEight, ZeroEight1) && r(EightEight2, EightEight3) && r(EightEight3, ZeroEight);        //green green1 red2 red3 
            var sc20 = r(ZeroEight, ZeroEight1) && r(EightEight2, FourEight3) && r(FourEight3, ZeroEight);      //green green1 red2 blue3 
            var sc21 = r(ZeroEight, ZeroEight1) && r(EightEight2, ZeroEight3) && r(ZeroEight3, ZeroEight);      //green green1 red2 green3 
            var sc22 = r(ZeroEight, ZeroEight1) && r(FourEight2, EightEight3) && r(EightEight3, ZeroEight);     //green green1 blue2 red3
            var sc23 = r(ZeroEight, ZeroEight1) && r(FourEight2, FourEight3) && r(FourEight3, ZeroEight);           //green green1 blue2 blue3 
            var sc24 = r(ZeroEight, ZeroEight1) && r(FourEight2, ZeroEight3) && r(ZeroEight3, ZeroEight);       //green green1 blue2 green3 
            var sc25 = r(ZeroEight, ZeroEight1) && r(ZeroEight2, EightEight3) && r(EightEight3, ZeroEight);     //green green1 green2 red3 
            var sc26 = r(ZeroEight, ZeroEight1) && r(ZeroEight2, FourEight3) && r(FourEight3, ZeroEight);       //green green1 green2 blue3 
            var sc27 = r(ZeroEight, ZeroEight1) && r(ZeroEight2, ZeroEight3) && r(ZeroEight3, ZeroEight);           //green green1 green2 green3 

            //scsfs (sc75s)
            var scsf1 = r(ZeroEight, EightEight1) && r(EightEight2, ZeroEight) && (!r(ZeroEight3, ZeroEight)) && (!r(FourEight3, ZeroEight));    // green red1 red2 !green3 !blue3
            var scsf2 = r(ZeroEight, EightEight1) && r(FourEight2, ZeroEight) && (!r(ZeroEight3, ZeroEight)) && (!r(FourEight3, ZeroEight));    // green red1 blue2 !green3 !blue3 
            var scsf3 = r(ZeroEight, EightEight1) && r(ZeroEight2, ZeroEight) && (!r(ZeroEight3, ZeroEight)) && (!r(FourEight3, ZeroEight));    // green red1 green2 !green3 !blue3 
            var scsf4 = r(ZeroEight, FourEight1) && r(EightEight2, ZeroEight) && (!r(ZeroEight3, ZeroEight)) && (!r(FourEight3, ZeroEight));    // green blue1 red2 !green3 !blue3 
            var scsf5 = r(ZeroEight, FourEight1) && r(FourEight2, ZeroEight) && (!r(ZeroEight3, ZeroEight)) && (!r(FourEight3, ZeroEight)); // green blue1 blue2 !green3 !blue3 
            var scsf6 = r(ZeroEight, FourEight1) && r(ZeroEight2, ZeroEight) && (!r(ZeroEight3, ZeroEight)) && (!r(FourEight3, ZeroEight)); // green blue1 green2 !green3 !blue3 
            var scsf7 = r(ZeroEight, ZeroEight1) && r(EightEight2, ZeroEight) && (!r(ZeroEight3, ZeroEight)) && (!r(FourEight3, ZeroEight));    // green green1 red2 !green3 !blue3 
            var scsf8 = r(ZeroEight, ZeroEight1) && r(FourEight2, ZeroEight) && (!r(ZeroEight3, ZeroEight)) && (!r(FourEight3, ZeroEight)); // green green1 blue2 !green3 !blue3 
            var scsf9 = r(ZeroEight, ZeroEight1) && r(ZeroEight2, ZeroEight) && (!r(ZeroEight3, ZeroEight)) && (!r(FourEight3, ZeroEight));    // green green1 green2 !green3 !blue3
            var scsf10 = r(ZeroEight1, EightEight2) && r(EightEight3, ZeroEight1) && (!r(ZeroEight, ZeroEight1)) && (!r(FourEight, ZeroEight1));    // green1 red2 red3 !green !blue
            var scsf11 = r(ZeroEight1, EightEight2) && r(FourEight3, ZeroEight1) && (!r(ZeroEight, ZeroEight1)) && (!r(FourEight, ZeroEight1)); // green1 red2 blue3 !green !blue 
            var scsf12 = r(ZeroEight1, EightEight2) && r(ZeroEight3, ZeroEight1) && (!r(ZeroEight, ZeroEight1)) && (!r(FourEight, ZeroEight1)); // green1 red2 green3 !green !blue 
            var scsf13 = r(ZeroEight1, FourEight2) && r(EightEight3, ZeroEight1) && (!r(ZeroEight, ZeroEight1)) && (!r(FourEight, ZeroEight1));     // green1 blue2 red3 !green !blue 
            var scsf14 = r(ZeroEight1, FourEight2) && r(FourEight3, ZeroEight1) && (!r(ZeroEight, ZeroEight1)) && (!r(FourEight, ZeroEight1));    // green1 blue2 blue3 !green !blue 
            var scsf15 = r(ZeroEight1, FourEight2) && r(ZeroEight3, ZeroEight1) && (!r(ZeroEight, ZeroEight1)) && (!r(FourEight, ZeroEight1));   // green1 blue2 green3 !green !blue 
            var scsf16 = r(ZeroEight1, ZeroEight2) && r(EightEight3, ZeroEight1) && (!r(ZeroEight, ZeroEight1)) && (!r(FourEight, ZeroEight1)); // green1 green2 red3 !green !blue 
            var scsf17 = r(ZeroEight1, ZeroEight2) && r(FourEight3, ZeroEight1) && (!r(ZeroEight, ZeroEight1)) && (!r(FourEight, ZeroEight1));  // green1 green2 blue3 !green !blue 
            var scsf18 = r(ZeroEight1, ZeroEight2) && r(ZeroEight3, ZeroEight1) && (!r(ZeroEight, ZeroEight1)) && (!r(FourEight, ZeroEight1));  // green1 green2 green3 !green !blue 

            //scsfs (sc50s)
            var sc501 = r(ZeroEight, EightEight1) && !r(ZeroEight, ZeroEight2) && !r(ZeroEight, ZeroEight3) && !r(ZeroEight, FourEight2) && !r(ZeroEight, FourEight3);     //green red1 !green2 !green3 !blue2 !blue3
            var sc502 = r(ZeroEight, FourEight1) && !r(ZeroEight, ZeroEight2) && !r(ZeroEight, ZeroEight3) && !r(ZeroEight, FourEight2) && !r(ZeroEight, FourEight3);      //green blue1 !green2 !green3 !blue2 !blue3 
            var sc503 = r(ZeroEight, ZeroEight1) && !r(ZeroEight, ZeroEight2) && !r(ZeroEight, ZeroEight3) && !r(ZeroEight, FourEight2) && !r(ZeroEight, FourEight3);      //green green1 !green2 !green3 !blue2 !blue3 
            var sc504 = r(ZeroEight1, EightEight2) && !r(ZeroEight1, ZeroEight) && !r(ZeroEight1, ZeroEight3) && !r(ZeroEight1, FourEight) && !r(ZeroEight1, FourEight3);      //green1 red2 !green !green3 !blue !blue3
            var sc505 = r(ZeroEight1, FourEight2) && !r(ZeroEight1, ZeroEight) && !r(ZeroEight1, ZeroEight3) && !r(ZeroEight1, FourEight) && !r(ZeroEight1, FourEight3);       //green1 blue2 !green !green3 !blue !blue3 
            var sc506 = r(ZeroEight1, ZeroEight2) && !r(ZeroEight1, ZeroEight) && !r(ZeroEight1, ZeroEight3) && !r(ZeroEight1, FourEight) && !r(ZeroEight1, FourEight3);       //green1 green2 !green !green3 !blue !blue3 
            var sc507 = r(ZeroEight2, EightEight3) && !r(ZeroEight2, ZeroEight) && !r(ZeroEight2, ZeroEight1) && !r(ZeroEight2, FourEight) && !r(ZeroEight2, FourEight1);      //green2 red3 !green !green1 !blue !blue1
            var sc508 = r(ZeroEight2, FourEight3) && !r(ZeroEight2, ZeroEight) && !r(ZeroEight2, ZeroEight1) && !r(ZeroEight2, FourEight) && !r(ZeroEight2, FourEight1);       //green2 red3 !green !green1 !blue !blue1
            var sc509 = r(ZeroEight2, ZeroEight3) && !r(ZeroEight2, ZeroEight) && !r(ZeroEight2, ZeroEight1) && !r(ZeroEight2, FourEight) && !r(ZeroEight2, FourEight1);        //green2 red3 !green !green1 !blue !blue1

            //sc25s
            var ValidSup25a = !r(ZeroEight, ZeroEight1) && !r(ZeroEight, ZeroEight2) && !r(ZeroEight, ZeroEight3) && !r(ZeroEight, FourEight) && !r(ZeroEight, FourEight1) && !r(ZeroEight, FourEight2) && !r(ZeroEight, FourEight3);  // !green1 !green2 !green3 !blue !blue1 !blue2 !blue3
            var ValidSup25b = !r(ZeroEight1, ZeroEight) && !r(ZeroEight1, ZeroEight2) && !r(ZeroEight1, ZeroEight3) && !r(ZeroEight1, FourEight) && !r(ZeroEight1, FourEight1) && !r(ZeroEight1, FourEight2) && !r(ZeroEight1, FourEight3); // !green  !green2 !green3 !blue !blue1 !blue2 !blue3
            var ValidSup25c = !r(ZeroEight2, ZeroEight) && !r(ZeroEight2, ZeroEight1) && !r(ZeroEight2, ZeroEight3) && !r(ZeroEight2, FourEight) && !r(ZeroEight2, FourEight1) && !r(ZeroEight2, FourEight2) && !r(ZeroEight2, FourEight3); // !green  !green1 !green3 !blue !blue1 !blue2 !blue3
            var ValidSup25d = !r(ZeroEight3, ZeroEight) && !r(ZeroEight3, ZeroEight1) && !r(ZeroEight3, ZeroEight2) && !r(ZeroEight3, FourEight) && !r(ZeroEight3, FourEight1) && !r(ZeroEight3, FourEight2) && !r(ZeroEight3, FourEight3); // !green  !green1 !green2 !blue !blue1 !blue2 !blue3

            // results
            var result = new SupportAndResistanceResult();

            result.ValidRes100 = (rc1 || rc2 || rc3 || rc4 || rc5 || rc6 || rc7 || rc8 || rc9 || rc10 || rc11 || rc12 || rc13 || rc14 || rc15 || rc16 || rc17 || rc18 || rc19 || rc20 || rc21 || rc22 || rc23 || rc24 || rc25 || rc26 || rc27);
            result.ValidRes75a = (rcsf1 || rcsf2 || rcsf3 || rcsf4 || rcsf5 || rcsf6 || rcsf7 || rcsf8 || rcsf9);
            result.ValidRes75b = (rcsf10 || rcsf11 || rcsf12 || rcsf13 || rcsf14 || rcsf15 || rcsf16 || rcsf17 || rcsf18);
            result.ValidRes50a = (rc501 || rc502 || rc503);
            result.ValidRes50b = (rc504 || rc505 || rc506);
            result.ValidRes50c = (rc507 || rc508 || rc509);

            result.ValidNeu100 = (nc1 || nc2 || nc3 || nc4 || nc5 || nc6 || nc7 || nc8 || nc9 || nc10 || nc11 || nc12 || nc13 || nc14 || nc15 || nc16 || nc17 || nc18 || nc19 || nc20 || nc21 || nc22 || nc23 || nc24 || nc25 || nc26 || nc27);
            result.ValidNeu75a = (ncsf1 || ncsf2 || ncsf3 || ncsf4 || ncsf5 || ncsf6 || ncsf7 || ncsf8 || ncsf9);
            result.ValidNeu75b = (ncsf10 || ncsf11 || ncsf12 || ncsf13 || ncsf14 || ncsf15 || ncsf16 || ncsf17 || ncsf18);
            result.ValidNeu50a = (nc501 || nc502 || nc503);
            result.ValidNeu50b = (nc504 || nc505 || nc506);
            result.ValidNeu50c = (nc507 || nc508 || nc509);

            result.ValidSup100 = (sc1 || sc2 || sc3 || sc4 || sc5 || sc6 || sc7 || sc8 || sc9 || sc10 || sc11 || sc12 || sc13 || sc14 || sc15 || sc16 || sc17 || sc18 || sc19 || sc20 || sc21 || sc22 || sc23 || sc24 || sc25 || sc26 || sc27);
            result.ValidSup75a = (scsf1 || scsf2 || scsf3 || scsf4 || scsf5 || scsf6 || scsf7 || scsf8 || scsf9);
            result.ValidSup75b = (scsf10 || scsf11 || scsf12 || scsf13 || scsf14 || scsf15 || scsf16 || scsf17 || scsf18);
            result.ValidSup50a = (sc501 || sc502 || sc503);
            result.ValidSup50b = (sc504 || sc505 || sc506);
            result.ValidSup50c = (sc507 || sc508 || sc509);

            return result;
        }

        private bool r(decimal a, decimal b)
        {
            return Math.Abs(a - b) <= SymbolMinTick;
        }
    }
}
