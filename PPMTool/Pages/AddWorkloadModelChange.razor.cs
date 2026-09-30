// SPDX-FileCopyrightText: 2026 University of Manchester
//
// SPDX-License-Identifier: apache-2.0

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using PPMTool.Data;
using PPMTool.Data.Entities;
using PPMTool.Data.Enums;
using PPMTool.Services;
using static PPMTool.Data.Extensions;

namespace PPMTool.Pages
{
    [Authorize(Roles = "Manager,Superuser")]
    public partial class AddWorkloadModelChange : AddPersonProperty<WorkloadModelChange>
    {
        [Inject]
        private FinancialReferenceService FinancialReferenceService { get; set; }

        private bool financeEnabled;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            financeEnabled = FeatureService.IsFeatureEnabled(FeatureType.ProjectFinance);

            if (PersonId > 0)
            {
                personModel = PersonService.GetById(Context, PersonId);
                dataGridEntities = personModel.WorkloadModelChanges.ToList();
            }
            else
            {
                dataGridEntities = new List<WorkloadModelChange>();
            }
            EditAuthorised = IsSuperuserOrLineManagerOfThisPerson(personModel);
            SetDefaultActionBar(HandleValidSubmit, DiscardChanges);

            LogInformation($"Viewing workload model changes for {personModel?.Name}");
        }

        protected override async Task InsertRow()
        {
            await base.InsertRow();
            entityToInsert.ChangeDate = DateTime.Today;

            // Set the default grade to 6 if not specified
            if (entityToInsert.Grade == 0)
            {
                entityToInsert.Grade = 6;
            }

            // Set the default cost value name based on the grade if finance is enabled and not specified
            if (financeEnabled && string.IsNullOrWhiteSpace(entityToInsert.CostValueName))
            {
                entityToInsert.CostValueName = FinancialReference.GetDefaultCostValueNameForGrade(entityToInsert.Grade);
            }

            await dataGrid.InsertRow(entityToInsert);
        }

        private void DiscardChanges()
        {
            LogInformation($"Discarding workload model changes!");

            // Just navigate away as nothing will have been written to the database
            Navigation.NavigateTo($"people/addperson/{PersonId}");
        }

        /// <summary>
        /// Gets the available cost value options for a given change date from the financial reference set associated with the change date.
        /// If finance is not enabled, returns an empty list.
        /// If no financial reference is available for the date, logs a warning and returns an empty list.
        /// </summary>
        /// <param name="changeDate"></param>
        /// <returns></returns>
        private IEnumerable<string> GetCostValueOptions(DateTime changeDate)
        {
            if (!financeEnabled)
            {
                return Enumerable.Empty<string>();
            }

            try
            {
                // Get the financial reference for the change date and return the distinct value names, excluding the recovery target
                var finRef = FinancialReferenceService.GetFinancialReferenceForDate(Context, changeDate);
                return finRef.Values
                    .Where(x => !string.IsNullOrWhiteSpace(x.ValueName)
                                && !x.ValueName.Equals(FinancialReference.RecoveryTargetName, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.ValueName.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList();
            }
            catch (FinancialRefException)
            {
                // This only happens if there is no financial reference available at all as it will map to the nearest one
                LogWarning($"No financial reference available for {changeDate:dd/MM/yyyy} while loading WLM cost key options.");
                return Enumerable.Empty<string>();
            }
        }

        private void HandleValidSubmit()
        {
            if (personModel != null)
            {
                // Check it doesn't duplicate the date, otherwise reject update
                if (dataGridEntities.DistinctBy(x => x.ChangeDate).Count() != dataGridEntities.Count())
                {
                    LogWarning($"Availability change duplicates a change date!");
                    SetErrorMessage(new StatusMessage("You cannot have multiple changes in availability on the same day!", StatusMessage.MessageType.Error));
                    return;
                }

                ClearErrorMessage();

                // Update the person model, save to database, refresh the list and reset the model
                personModel.WorkloadModelChanges.Clear();
                foreach (var avail in dataGridEntities)
                {
                    personModel.WorkloadModelChanges.Add(avail);
                }

                LogInformation($"Saving workload model changes for {personModel.Name}.");
                PersonService.Update(Context, personModel);
                Navigation.NavigateTo($"people/addperson/{PersonId}");
            }
        }
    }
}
