using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using hpMvc.DataBase;
using hpMvc.Models;

namespace hpMvc.Services.GIRUpdateService
{
    public static class GIRUpdateService
    {
        public static bool UpdateAll()
        {
            var calStudyInfoList = CalorieCalc.GetCalStudyInfoAll();
            foreach(var calStudyInfo in calStudyInfoList)
            {
                if (calStudyInfo.Hours == null || calStudyInfo.Hours == 0 || calStudyInfo.Weight == null || calStudyInfo.Weight == 0)
                {
                    continue;
                }
                var gir = GetGirForCalStudyInfo(calStudyInfo);
                var dto = CalorieCalc.UpdateCalStudyInfoGir(calStudyInfo);
            }
            return true;
        }

        public static double UpdateForCalStudyId(string id)
        {
            var calStudyInfo = CalorieCalc.GetCalStudyInfo(id);
            var gir = GetGirForCalStudyInfo(calStudyInfo);
            var dto = CalorieCalc.UpdateCalStudyInfoGir(calStudyInfo);
            return gir;
        }

        public static double UpdateForStudyIdandDate(string studyId, string calcDate)
        {
            var id = CalorieCalc.GetCalStudyId(studyId, calcDate);
            var calStudyInfo = CalorieCalc.GetCalStudyInfo(id.ToString());
            if (calStudyInfo.Hours == null || calStudyInfo.Hours == 0 || calStudyInfo.Weight == null || calStudyInfo.Weight == 0)
            {
                return 0;
            }
            var gir = GetGirForCalStudyInfo(calStudyInfo);
            
            return gir;
        }
        public static double GetGirForCalStudyInfo(CalStudyInfo csi)
        {
            var allData = GetAllDataForRecalc(csi.StudyId.ToString(), csi.CalcDate);
            double totInfusions = 0;
            double totParenProtein = 0;
            double totParenCho = 0;
            double totParenLipid = 0;
            double totEnProtein = 0;
            double totEnCho = 0;
            double totEnLipid = 0;
            
            foreach(var ci in allData.calInfusionCol)
            {
                foreach(var vol in ci.Volumes)
                {
                    totInfusions += vol * ci.DexValue;
                }
            }

            foreach(var par in allData.calParenterals)
            {
                double lipVal = 0;
                totParenProtein += (par.AminoPercent  * par.Volume * .04);
                if(par.DexPercent >0){
                    totParenCho += (par.DexPercent *  par.Volume * .034);
                }
                else
                {
                    if (par.LipidPercent == 10)
                    {
                        lipVal = 1.1;
                    }
                    else if (par.LipidPercent == 20)
                    {
                        lipVal = 2.0;
                    }
                    else if (par.LipidPercent == 30)
                    {                        
                        lipVal = 3.0;
                    }

                    totParenLipid += lipVal * par.Volume;
                }
                
            }

            foreach(var ent in allData.calEnterals)
            {
                var kCals = ent.KcalMl * ent.Volume;
                totEnProtein += kCals * (ent.ProteinPercent / 100);
                totEnCho += kCals * (ent.ChoPercent / 100);
                totEnLipid += kCals * (ent.LipidPercent / 100);
            }

            foreach(var add in allData.calAdditives)
            {
                var kCals = add.KcalUnit * add.Volume;
                totEnProtein += kCals * (add.ProteinPercent / 100);
                totEnCho += kCals * (add.ChoPercent / 100);
                totEnLipid += kCals * (add.LipidPercent / 100);
            }

            //calculate gir
            var totalEntPar = totParenProtein + totParenCho + totParenLipid +
                totEnProtein + totEnCho + totEnLipid;
            
            var choKcals = totEnCho;
            var choMg = (choKcals / 4) * 1000;
            var dexKal = totInfusions + totParenCho;
            var dexMg = (dexKal / 3.4) * 1000;
            csi.Gir = ((choMg + dexMg) / csi.Weight) / (csi.Hours * 60); 
            return csi.Gir;
        }
        

        public static CalAllData GetAllDataForRecalc(string studyID, string calcDate)
        {
            var cad = new CalAllData();
            var cicl = new List<CalInfuseColumn>();

            int calStudyID = CalorieCalc.GetCalStudyId(studyID, calcDate);

            var cidl = CalorieCalc.GetCalInfusionsDexData(calStudyID);

            foreach (var cid in cidl)
            {
                List<CalInfusionVol> civl = CalorieCalc.GetCalInfusionsVolData(cid.ID);
                int[] volumes = new int[civl.Count];
                int index = 0;
                var cic = new CalInfuseColumn();

                foreach (var civ in civl)
                {

                    cic.DexValue = cid.DexVal;
                    volumes[index] = civ.Volume;
                    index++;
                }
                cic.Volumes = volumes;
                cicl.Add(cic);
            }

            var cpl = CalorieCalc.GetCalParenteralsData(calStudyID);
            var cel = CalorieCalc.GetCalEnteralsData(calStudyID);
            foreach(var ent in cel)
            {
                CalorieCalc.GetFormularData(ent);
            }
            var cal = CalorieCalc.GetCalAdditivesData(calStudyID);
            foreach(var add in cal)
            {
                CalorieCalc.GetAdditiveData(add);
            }
            var con = CalorieCalc.GetCalOtherNutrition(calStudyID);


            cad.calInfusionCol = cicl;
            cad.calParenterals = cpl;
            cad.calEnterals = cel;
            cad.calAdditives = cal;
            cad.calOtherNutrition = con;

            return cad;
        }

    }
}