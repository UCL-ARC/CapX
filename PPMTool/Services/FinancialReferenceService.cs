// SPDX-FileCopyrightText: 2026 University of Manchester
//
// SPDX-License-Identifier: apache-2.0

using Microsoft.EntityFrameworkCore;
using PPMTool.Data;
using PPMTool.Data.Context;
using PPMTool.Data.Entities;

namespace PPMTool.Services
{
    public class FinancialReferenceService : BaseEntityService<FinancialReference>
    {
        public FinancialReferenceService(ILogger<FinancialReferenceService> logger) : base(logger)
        {
        }

        public override int Add(PPMToolContext context, FinancialReference entity, bool commitChanges = true)
        {
            if (DuplicateDetected(context, entity))
            {
                return -1;
            }
            context.FinancialReferences.Add(entity);
            if (commitChanges) CommitChanges(context);
            return entity.FinancialReferenceId;
        }

        public override void Delete(PPMToolContext context, FinancialReference entity, bool commitChanges = true)
        {
            // Delete all associated FinancialReferenceValues before deleting the FinancialReference entity
            var values = context.FinancialReferenceValues.Where(x => x.FinancialReferenceId == entity.FinancialReferenceId);
            context.FinancialReferenceValues.RemoveRange(values);

            // Now delete the FinancialReference entity
            context.FinancialReferences.Remove(entity);
            if (commitChanges) CommitChanges(context);
        }

        /// <summary>
        /// Retrieves all financial reference entities from the specified database context. Will return an empty list if no references exist.
        /// </summary>
        /// <param name="context">The database context used to access financial reference entities. Cannot be null.</param>
        /// <returns>An enumerable collection of all financial reference entities in the context.</returns>
        public override IEnumerable<FinancialReference> GetAll(PPMToolContext context)
        {
            // Retrieve all financial references and include their associated values
            return context.FinancialReferences
                .OrderBy(x => x.FinancialYear)
                .Include(x => x.Values);
        }

        /// <summary>
        /// Returns the Financial References from the db. If none have been added then the app will
        /// crash in certain places if the Finance Feature is not enabled. The check in this method
        /// helps avoid this exception by passing back a non-null, non-zero IEnumerable to satisfy
        /// the requesting call. This was added to allow bypassing of the crash when a new Project
        /// was added without the Finance Feature being enabled (so no Financial References exist).
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IEnumerable<FinancialReference> GetAllOrDefault(PPMToolContext context)
        {
            // If DB table is empty return a list of one default item to avoid a financial ref exception
            if (!context.FinancialReferences.Any())
            {
                return new List<FinancialReference> { new FinancialReference() };
            }

            // Default to the standard call if there are references in the list
            return GetAll(context);
        }

        public override int Update(PPMToolContext context, FinancialReference entity, bool commitChanges = true)
        {
            if (DuplicateDetected(context, entity))
            {
                return -1;
            }
            context.FinancialReferences.Update(entity);
            if (commitChanges) CommitChanges(context);
            return entity.FinancialReferenceId;
        }

        /// <summary>
        /// Checks for duplicate financial references based on the financial year and value names. Returns true if a duplicate is detected, otherwise false.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override bool DuplicateDetected(PPMToolContext context, FinancialReference entity)
        {
            var duplicateYear = context.FinancialReferences.Any(x => x.FinancialYear == entity.FinancialYear && x.FinancialReferenceId != entity.FinancialReferenceId);
            var values = entity.Values ?? new List<FinancialReferenceValue>();

            // Considered a duplicate reference set if any of the value names are the same (case insensitive) and not null or whitespace
            var duplicateValueNames = values
                .Where(x => !string.IsNullOrWhiteSpace(x.ValueName))
                .GroupBy(x => x.ValueName.Trim().ToLower())
                .Any(x => x.Count() > 1);

            return duplicateYear || duplicateValueNames;
        }

        /// <summary>
        /// Retrieves a financial reference entity by its primary key from the specified database context. Returns null if no matching entity is found.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="financialReferenceId"></param>
        /// <returns></returns>
        internal FinancialReference GetById(PPMToolContext context, int financialReferenceId)
        {
            return GetAll(context).FirstOrDefault(x => x.FinancialReferenceId == financialReferenceId);
        }

        /// <summary>
        /// Gets the available cost value names from the suitable financial reference set for the provided date.
        /// Excludes recovery target by default.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="date"></param>
        /// <param name="includeRecoveryTarget"></param>
        /// <returns></returns>
        public IEnumerable<string> GetCostValueOptionsForDate(PPMToolContext context, DateTime date, bool includeRecoveryTarget = false)
        {
            if (context == null || !context.FinancialReferences.Any())
            {
                return Enumerable.Empty<string>();
            }

            var finRef = GetFinancialReferenceForDate(context, date);
            return GetCostValueOptions(finRef, includeRecoveryTarget);
        }

        /// <summary>
        /// Gets the available cost value names from the provided financial reference set.
        /// Excludes recovery target by default.
        /// </summary>
        /// <param name="finRef"></param>
        /// <param name="includeRecoveryTarget"></param>
        /// <returns></returns>
        public IEnumerable<string> GetCostValueOptions(FinancialReference finRef, bool includeRecoveryTarget = false)
        {
            if (finRef?.Values == null)
            {
                return Enumerable.Empty<string>();
            }

            var values = finRef.Values
                .Where(x => !string.IsNullOrWhiteSpace(x.ValueName));

            if (!includeRecoveryTarget)
            {
                values = values.Where(x => !x.ValueName.Equals(FinancialReference.RecoveryTargetName, StringComparison.OrdinalIgnoreCase));
            }

            return values
                .Select(x => x.ValueName.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        }

        /// <summary>
        /// Method to return a suitable financial reference following set logic given a date in a certain financial year
        /// </summary>
        /// <param name="context"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public FinancialReference GetFinancialReferenceForDate(PPMToolContext context, DateTime startDate)
        {
            return GetAll(context).GetSuitableFinancialReference(startDate);
        }
    }
}
