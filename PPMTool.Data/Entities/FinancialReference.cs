// SPDX-FileCopyrightText: 2026 University of Manchester
//
// SPDX-License-Identifier: apache-2.0

using Microsoft.Extensions.Logging;
using PPMTool.Data.Interfaces;

namespace PPMTool.Data.Entities
{
    public class FinancialReference : ILoggableObject
    {
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
        public float GetValue(string? valueName, ILogger? logger = null)
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
        /// Gets the annual cost for a given workload model using its explicitly selected cost value name.
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

            if (string.IsNullOrWhiteSpace(workloadModel.CostValueName))
            {
                logger?.LogWarning("FinancialReference [{FinancialReferenceId}] - {FinancialYear}: workload model {WorkloadModelChangeId} has no cost key selected. Returning 0.", FinancialReferenceId, FinancialYear, workloadModel.WorkloadModelChangeId);
                return 0;
            }

            return GetValue(workloadModel.CostValueName, logger);
        }
    }
}
