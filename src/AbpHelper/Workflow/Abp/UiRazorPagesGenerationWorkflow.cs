﻿using System.Collections.Generic;
using EasyAbp.AbpHelper.Steps.Abp;
using EasyAbp.AbpHelper.Steps.Abp.ModificationCreatorSteps;
using EasyAbp.AbpHelper.Steps.Common;
using Elsa;
using Elsa.Activities.ControlFlow.Activities;
using Elsa.Expressions;
using Elsa.Scripting.JavaScript;
using Elsa.Services;

namespace EasyAbp.AbpHelper.Workflow.Abp
{
    public static class UiRazorPagesGenerationWorkflow
    {
        public static IActivityBuilder AddUiRazorPagesGenerationWorkflow(this IOutcomeBuilder builder)
        {
            return builder
                    /* Generate razor pages ui files*/
                    .Then<TemplateGroupGenerationStep>(
                        step =>
                        {
                            step.GroupName = "UiRazor";
                            step.TargetDirectory = new JavaScriptExpression<string>("AspNetCoreDir");
                        }
                    )
                    /* Add menu */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new LiteralExpression("*MenuContributor.cs")
                    )
                    .Then<MenuContributorStep>()
                    .Then<FileModifierStep>()
                    /* Add mapping */
                    .Then<FileFinderStep>(
                        step => step.SearchFileName = new LiteralExpression<string>("*WebAutoMapperProfile.cs")
                    )
                    .Then<WebAutoMapperProfileStep>()
                    .Then<FileModifierStep>()
                    /* Add localization */
                    .Then<TextGenerationStep>(
                        step => { step.TemplateName = "Localization"; }
                    )
                    .Then<MultiFileFinderStep>(
                        step =>
                        {
                            step.SearchFileName = new LiteralExpression("*.json");
                            step.BaseDirectory = new JavaScriptExpression<string>(@"`${BaseDirectory}\\src\\${ProjectInfo.FullName}.Domain.Shared\\Localization`");
                        }
                    )
                    .Then<ForEach>(
                        x => { x.CollectionExpression = new JavaScriptExpression<IList<object>>(MultiFileFinderStep.DefaultFileParameterName); },
                        branch =>
                            branch.When(OutcomeNames.Iterate)
                                .Then<LocalizationJsonModificationCreatorStep>(
                                    step =>
                                    {
                                        step.TargetFile = new JavaScriptExpression<string>("CurrentValue");
                                        step.LocalizationJson = new JavaScriptExpression<string>(TextGenerationStep.DefaultGeneratedTextParameterName);
                                    }
                                )
                                .Then(branch)
                    )
                ;
        }
    }
}