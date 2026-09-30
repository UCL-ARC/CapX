// SPDX-FileCopyrightText: 2026 University of Manchester
//
// SPDX-License-Identifier: apache-2.0

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using PPMTool.Data;
using PPMTool.Data.Entities;
using PPMTool.Services;

namespace PPMTool.Pages
{
    [Authorize(Roles = "Superuser")]
    public partial class AddFinancialReference : DataGridPage<FinancialReferenceValue>
    {
        [Parameter]
        public int FinancialReferenceId { get; set; }

        [Inject]
        private FinancialReferenceService FinancialReferenceService { get; set; }

        private FinancialReference financialReference;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            if (FinancialReferenceId > 0)
            {
                financialReference = FinancialReferenceService.GetById(Context, FinancialReferenceId);
                dataGridEntities = financialReference?.Values?.ToList() ?? new List<FinancialReferenceValue>();
            }
            else
            {
                financialReference = new FinancialReference();
                dataGridEntities = new List<FinancialReferenceValue>();
            }

            SetDefaultActionBar(HandleValidSubmit, DiscardChanges);
            LogInformation($"Adding / Editing financial reference set {financialReference?.GetSensibleObjectName()}");
        }

        /// <summary>
        /// Discards changes and navigates back to the manage financial references page.
        /// </summary>
        private void DiscardChanges()
        {
            LogInformation("Discarding financial reference set changes");
            Navigation.NavigateTo("managefinref");
        }

        /// <summary>
        /// Handles the valid submission of the financial reference form. Validates the input and either updates or adds the financial reference set.
        /// </summary>
        private void HandleValidSubmit()
        {
            if (financialReference == null)
            {
                return;
            }

            // Clear any previous error messages and reset the financial reference values stored on the model
            ClearErrorMessage();
            financialReference.Values.Clear();

            // Trim whitespace from value names and associate each value with the financial reference set
            foreach (var value in dataGridEntities)
            {
                value.ValueName = value.ValueName?.Trim() ?? string.Empty;
                value.FinancialReference = financialReference;
                financialReference.Values.Add(value);
            }

            // Validate that all values have a name
            if (financialReference.Values.Any(x => string.IsNullOrWhiteSpace(x.ValueName)))
            {
                SetErrorMessage(new StatusMessage("All values must have a name.", StatusMessage.MessageType.Error));
                return;
            }

            // Validation on duplicate sets by year or duplicate value names within the set happens in the service
            int result;
            if (financialReference.FinancialReferenceId != 0)
            {
                result = FinancialReferenceService.Update(Context, financialReference);
            }
            else
            {
                result = FinancialReferenceService.Add(Context, financialReference);
            }

            // If the result is -1, it indicates a validation failure due to duplicate financial year or duplicate value names within the set
            if (result == -1)
            {
                SetErrorMessage(new StatusMessage("Financial year must be unique and value names cannot duplicate in a set.", StatusMessage.MessageType.Error));
                return;
            }

            // If the result is 0, it indicates a failure to save the financial reference set
            if (result == 0)
            {
                SetErrorMessage(new StatusMessage("Failed to save the financial reference set.", StatusMessage.MessageType.Error));
                return;
            }

            // Success so navigate back to the manage financial references page
            Navigation.NavigateTo("managefinref");
        }

        /// <summary>
        /// Cancels the edit operation for a given financial reference value. Restores the original state of the entity and cancels the edit row in the data grid.
        /// </summary>
        /// <param name="entity"></param>
        protected override void CancelEdit(FinancialReferenceValue entity)
        {
            LogInformation($"Cancel edit row for {entity.GetSensibleObjectName()}");
            Reset();
            FinancialReferenceService.RestoreModel(Context, ref entity);
            dataGrid.CancelEditRow(entity);
        }

        /// <summary>
        /// Handles the creation of a new financial reference value. Associates the new value with the current financial reference set and adds it to the data grid entities.
        /// </summary>
        /// <param name="entity"></param>
        protected override void OnCreateRow(FinancialReferenceValue entity)
        {
            entity.FinancialReference = financialReference;
            dataGridEntities.Add(entity);
            entityToInsert = null;
        }

        /// <summary>
        /// Handles the update of an existing financial reference value. Associates the updated value with the current financial reference set and resets the data grid state.
        /// </summary>
        /// <param name="entity"></param>
        protected override void OnUpdateRow(FinancialReferenceValue entity)
        {
            entity.FinancialReference = financialReference;
            Reset();
        }
    }
}
