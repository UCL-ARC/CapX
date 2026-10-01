// SPDX-FileCopyrightText: 2026 University of Manchester
//
// SPDX-License-Identifier: apache-2.0

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using PPMTool.Data;
using PPMTool.Data.Entities;
using PPMTool.Data.Enums;
using PPMTool.Services;

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

            // Cost value key relationship can be selected when Project Finance is enabled
            if (financeEnabled)
            {
                entityToInsert.CostValueSetId = null;
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
        /// Gets the available cost value options for a given change date from the suitable financial reference set.
        /// </summary>
        /// <param name="changeDate"></param>
        /// <returns></returns>
        private IEnumerable<FinancialReferenceValue> GetCostValueOptions(DateTime changeDate)
        {
            if (!financeEnabled)
            {
                return Enumerable.Empty<FinancialReferenceValue>();
            }

            return FinancialReferenceService.GetCostValueOptionsForDate(Context, changeDate);
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

                // Log warning if saving with missing cost keys while Project Finance is enabled, as this will use zero-cost fallback in finance calculations
                if (financeEnabled && dataGridEntities.Any(x => x.CostValueSetId == null))
                {
                    LogWarning("Saving WLM changes with one or more missing cost key links while Project Finance is enabled. Missing links will use zero-cost fallback in finance calculations.");
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
