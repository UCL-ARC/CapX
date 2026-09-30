// SPDX-FileCopyrightText: 2026 University of Manchester
//
// SPDX-License-Identifier: apache-2.0

using System.ComponentModel.DataAnnotations;
using PPMTool.Data.Interfaces;

namespace PPMTool.Data.Entities
{
    /// <summary>
    /// Represents a key-value pair of financial reference data that belongs to a specific financial year set.
    /// </summary>
    public class FinancialReferenceValue : ILoggableObject
    {
        public int FinancialReferenceValueId { get; set; }

        [Required]
        public string ValueName { get; set; } = null!;

        public float Value { get; set; }

        [Required]
        public virtual FinancialReference FinancialReference { get; set; } = null!;

        public int FinancialReferenceId { get; set; }

        /// <summary>
        /// Gets a sensible name for the object.
        /// </summary>
        /// <returns>The sensible name.</returns>
        public string GetSensibleObjectName()
        {
            return $"{FinancialReference?.GetSensibleObjectName()} | {ValueName}";
        }
    }
}
