using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace hpMvc.Models
{
    public class CalInfuseColumn
    {
        public double DexValue { get; set; }
        public int[] Volumes { get; set; }
       
    }

    public class CalAllData
    {
        public CalStudyInfo callstudyInfo { get; set; }
        public List<CalInfuseColumn> calInfusionCol { get; set; }
        public List<CalParenteral> calParenterals { get; set; }
        public List<CalEnteral> calEnterals { get; set; }
        public List<CalAdditive> calAdditives { get; set; }
        public CalOtherNutrition calOtherNutrition { get; set; }
    }
        
    public class CalStudyInfo
    {
        public int Id { get; set; }
        public int StudyId { get; set; }
        public string SStudyId { get; set; }
        public double Weight { get; set; }
        public double Gir { get; set; }
        public string CalcDate { get; set; }
        public int TotalCals { get; set; }
        public int Hours { get; set; }
    }

    public class CalOtherNutrition
    {
        public int Id { get; set; }
        public int CalStudyId { get; set; }
        public bool BreastFeeding { get; set; }
        public bool SolidFoods { get; set; }
        public bool Drinks { get; set; }
        public bool Other { get; set; }
        public string OtherText { get; set; }
    }

    public class CalEnteral
    {
        public int ID { get; set; }
        public int CalStudyID { get; set; }
        public int FormulaID { get; set; }
        public double Volume { get; set; }

        public double KcalMl { get; set; }
        public double ProteinPercent { get; set; }
        public double ChoPercent { get; set; }
        public double LipidPercent { get; set; }
    }

    public class CalAdditive
    {
        public int ID { get; set; }
        public int CalStudyID { get; set; }
        public int AdditiveID { get; set; }
        public double Volume { get; set; }
    }
        
    public class CalInfusionDex
    {
        public int ID { get; set; }
        public int CalStudyID { get; set; }
        public double DexVal { get; set; }        
    }

    public class CalInfusionVol
    {
        public int ID { get; set; }
        public int DexID { get; set; }
        public int Volume { get; set; }
    }

    public class CalParenteral
    {
        public int ID { get; set; }
        public int CalStudyID { get; set; }
        public double DexPercent { get; set; }
        public double AminoPercent { get; set; }
        public double LipidPercent { get; set; }
        public double Volume { get; set; }
    }

    public class DextroseConcentration
    {
        public int ID { get; set; }
        public string Concentration { get; set; }
        public double Kcal_ml { get; set; }
    }

    public class EnteralFormula : IValidatableObject
    {
        public int ID { get; set; }
        [Display (Name = "Formula Name")]
        [Required]
        public string Name { get; set; }
        [Display(Name = "kCal per unit")]
        [Required]
        public double Kcal_ml { get; set; }
        [Display(Name = "Protein % of kcal")]
        [Required]
        public double ProteinKcal { get; set; }
        [Display(Name = "CHO % of kcal")]
        [Required]
        public double ChoKcal { get; set; }
        [Display(Name = "Lipid % of kcal")]
        [Required]
        public double LipidKcal { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Math.Abs(ProteinKcal + ChoKcal + LipidKcal - 100) > 0)
            {
                var proteinfield = new[] {"ProteinKcal"};
                yield return
                    new ValidationResult("Protein % of kcal + CHO % of kcal + Lipid % of kcal must = 100", proteinfield)
                    ;

            }
        }
    }

    public class Additive : IValidatableObject
    {
        public int ID { get; set; }
        [Display(Name = "Additive Name")]
        [Required]
        public string Name { get; set; }
        public int Unit { get; set; }
        public string UnitName { get; set; }
        [Display(Name = "kCal per unit")]
        [Required]
        public double Kcal_unit { get; set; }
        [Display(Name = "Protein % of kcal")]
        [Required]
        public double ProteinKcal { get; set; }
        [Display(Name = "CHO % of kcal")]
        [Required]
        public double ChoKcal { get; set; }
        [Display(Name = "Lipid % of kcal")]
        [Required]
        public double LipidKcal { get; set; }
        public IEnumerable<IDandName> Units { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Math.Abs(ProteinKcal + ChoKcal + LipidKcal - 100) > 0)
            {
                var proteinfield = new[] { "ProteinKcal" };
                yield return
                    new ValidationResult("Protein % of kcal + CHO % of kcal + Lipid % of kcal must = 100", proteinfield)
                    ;

            }
        }
    }
}