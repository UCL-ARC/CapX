// SPDX-FileCopyrightText: 2026 University of Manchester
//
// SPDX-License-Identifier: apache-2.0

using Microsoft.Extensions.Logging;
using PPMTool.Data.Enums;
using PPMTool.Data.Interfaces;

namespace PPMTool.Data.Entities
{
    public class FinancialReference : ILoggableObject
    {
        public const string Grade41CostsName = "Grade41Costs";
        public const string Grade51CostsName = "Grade51Costs";
        public const string Grade55CostsName = "Grade55Costs";
        public const string Grade65CostsName = "Grade65Costs";
        public const string Grade71CostsName = "Grade71Costs";
        public const string Grade75CostsName = "Grade75Costs";
        public const string RecoveryTargetName = "RecoveryTarget";

        public int FinancialReferenceId { get; set; }

        /// <summary>
        /// Unique financial year that identifies this set of financial reference values.
        /// </summary>
        public int FinancialYear { get; set; } = DateTime.Today.Year;

        /// <summary>
        /// Flexible key-value financial reference values that belong to this financial year set.
        /// </summary>
        public virtual ICollection<FinancialReferenceValue> Values { get; set; } = new List<FinancialReferenceValue>();

        /// <summary>
        /// Helper to get a financial year from a DateTime
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static int GetFinancialYear(DateTime date)
        {
            return date.Date.Month < 8 ? date.Date.Year - 1 : date.Date.Year;
        }

        /// <summary>
        /// Returns a number between 0 and 1 depending on how much of a financial year takes place within the given window
        /// </summary>
        /// <param name="currentFY"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <exception cref="ArgumentException">If start date is after end date</exception>
        /// <returns></returns>
        public static float GetProportionOfFinancialYearInRange(int currentFY, DateTime startDate, DateTime endDate)
        {
            var startFY = new DateTime(currentFY, 8, 1);
            var endFY = new DateTime(currentFY + 1, 7, 31);
            if (startDate.Date > endDate.Date) throw new ArgumentException("Start Date is after the End Date!");

            // Range starts before the FY
            if (startDate.Date < startFY)
            {
                // Range starts and ends before FY starts
                if (endDate.Date < startFY)
                {
                    return 0;
                }

                // Range starts before FY starts but ends in middle of FY
                else if (endDate.Date <= endFY)
                {
                    return (float)endDate.Subtract(startFY).TotalDays / 365f;
                }

                // Range starts before FY starts and ends after FY ends so range spans whole FY
                else
                {
                    return 1f;
                }
            }

            // Range starts in FY
            else if (startDate.Date <= endFY)
            {
                // Range starts and ends within FY
                if (endDate.Date <= endFY)
                {
                    return (float)endDate.Date.Subtract(startDate.Date).TotalDays / 365f;
                }

                // Range starts within FY and ends after FY ends
                else
                {
                    return (float)endFY.Subtract(startDate.Date).TotalDays / 365f;
                }
            }

            // Range starts after FY ends
            return 0f;
        }

        public string GetSensibleObjectName()
        {
            return $"Financial Reference [{FinancialReferenceId}] - {FinancialYear}";
        }

        /// <summary>
        /// Gets a value from the financial reference setby name, returning 0 if not found or if the key is null/empty.
        /// Logs warnings if the key is missing or if the values collection is null.
        /// </summary>
        /// <param name="valueName"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public float GetValue(string valueName, ILogger? logger = null)
        {
            if (string.IsNullOrWhiteSpace(valueName))
            {
                logger?.LogWarning("FinancialReference [{FinancialReferenceId}] - {FinancialYear}: requested value with null/empty key. Returning 0.", FinancialReferenceId, FinancialYear);
                return 0f;
            }

            if (Values == null)
            {
                logger?.LogWarning("FinancialReference [{FinancialReferenceId}] - {FinancialYear}: values collection is null when looking up '{ValueName}'. Returning 0.", FinancialReferenceId, FinancialYear, valueName);
                return 0f;
            }

            var match = Values.FirstOrDefault(x =>
                !string.IsNullOrWhiteSpace(x.ValueName)
                && x.ValueName.Trim().Equals(valueName.Trim(), StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                logger?.LogWarning("FinancialReference [{FinancialReferenceId}] - {FinancialYear}: missing key '{ValueName}'. Returning 0.", FinancialReferenceId, FinancialYear, valueName);
                return 0f;
            }

            return match.Value;
        }

        /// <summary>
        /// Gets the recovery target value from the financial reference set.
        /// </summary>
        /// <returns></returns>
        public float GetRecoveryTarget()
        {
            return GetValue(RecoveryTargetName);
        }

        /// <summary>
        /// Gets the default cost value name for a given grade, based on predefined mappings.
        /// </summary>
        /// <param name="grade"></param>
        /// <returns></returns>
        public static string GetDefaultCostValueNameForGrade(int grade)
        {
            if (grade <= 4)
            {
                return Grade41CostsName;
            }
            else if (grade == 5)
            {
                return Grade55CostsName;
            }
            else if (grade == 6)
            {
                return Grade65CostsName;
            }

            return Grade75CostsName;
        }

        /// <summary>
        /// Gets the annual cost for a given workload model, using the cost value name specified in the workload model or a default based on the grade if not specified.
        /// </summary>
        /// <param name="workloadModel"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public double GetAnnualCostForWorkloadModel(WorkloadModelChange workloadModel, ILogger? logger = null)
        {
            if (workloadModel == null)
            {
                logger?.LogWarning("FinancialReference [{FinancialReferenceId}] - {FinancialYear}: workload model was null while resolving annual cost. Returning 0.", FinancialReferenceId, FinancialYear);
                return 0;
            }

            var valueName = string.IsNullOrWhiteSpace(workloadModel.CostValueName)
                ? GetDefaultCostValueNameForGrade(workloadModel.Grade)
                : workloadModel.CostValueName;

            return GetValue(valueName, logger);
        }

        /// <summary>
        /// Gets a suitable standard or junior figure from the financial references for annual costs
        /// </summary>
        /// <param name="rate"></param>
        /// <returns></returns>
        public double GetJuniorOrStandardAnnualCosts(Rate rate)
        {
            // Junior Rate
            if (rate == Rate.Junior)
            {
                return GetValue(Grade51CostsName);
            }

            // Standard Rate
            else if (rate == Rate.Standard)
            {
                return GetValue(Grade71CostsName);
            }

            // Senior rate
            else
            {
                return GetValue(Grade75CostsName);
            }
        }

        /// <summary>
        /// Returns the mid grade salary costs from the reference.
        /// Grade 4 always bottom of grade.
        /// Less than Grade4 returns G4.1.
        /// Greater than Grade 7 returns G7.1.
        /// </summary>
        /// <param name="grade"></param>
        /// <returns></returns>
        public double GetMidGradeCosts(int grade)
        {
            if (grade <= 4)
            {
                return GetValue(Grade41CostsName);
            }
            else if (grade == 5)
            {
                return GetValue(Grade55CostsName);
            }
            else if (grade == 6)
            {
                return GetValue(Grade65CostsName);
            }
            return GetValue(Grade75CostsName);
        }
    }
}
