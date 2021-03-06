﻿// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Management.Automation;
using Microsoft.Azure.Management.SiteRecovery.Models;
using Microsoft.Azure.Portal.RecoveryServices.Models.Common;
using Microsoft.WindowsAzure.Commands.Common.Properties;
using Properties = Microsoft.Azure.Commands.SiteRecovery.Properties;
using System.Collections.Generic;

namespace Microsoft.Azure.Commands.SiteRecovery
{
    /// <summary>
    /// Creates Azure Site Recovery Policy object in memory.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AzureRmSiteRecoverySite", DefaultParameterSetName = ASRParameterSets.Default)]
    [OutputType(typeof(List<ASRSite>))]
    public class GetAzureSiteRecoverySite : SiteRecoveryCmdletBase
    {
        #region Parameters
        /// <summary>
        /// Gets or sets Name of the Site.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByName, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Friendly name of the Site.
        /// </summary>
        [Parameter(ParameterSetName = ASRParameterSets.ByFriendlyName, Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public string FriendlyName { get; set; }
        #endregion Parameters

        /// <summary>
        /// ProcessRecord of the command.
        /// </summary>
        public override void ExecuteCmdlet()
        {
            try
            {
                switch (this.ParameterSetName)
                {
                    case ASRParameterSets.ByName:
                        this.GetByName();
                        break;
                    case ASRParameterSets.ByFriendlyName:
                        this.GetByFriendlyName();
                        break;
                    case ASRParameterSets.Default:
                        this.GetAll();
                        break;
                }
            }
            catch (Exception exception)
            {
                this.HandleException(exception);
            }
        }

        /// <summary>
        /// Queries by friendly name.
        /// </summary>
        private void GetByFriendlyName()
        {
            FabricListResponse fabricListResponse =
                RecoveryServicesClient.GetAzureSiteRecoveryFabric();

            bool found = false;
            foreach (Fabric fabric in fabricListResponse.Fabrics)
            {
                // Do not process for fabrictype other than HyperVSite 
                if (String.Compare(fabric.Properties.CustomDetails.InstanceType, Constants.HyperVSite) != 0)
                    continue;

                if (0 == string.Compare(this.FriendlyName, fabric.Properties.FriendlyName, true))
                {
                    this.WriteSite(fabric);
                    found = true;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException(
                    string.Format(
                    Properties.Resources.SiteNotFound,
                    this.FriendlyName,
                    PSRecoveryServicesClient.asrVaultCreds.ResourceName));
            }
        }

        /// <summary>
        /// Queries by name.
        /// </summary>
        private void GetByName()
        {
            FabricResponse fabricResponse =
                RecoveryServicesClient.GetAzureSiteRecoveryFabric(this.Name);
            bool found = false;

            if (fabricResponse != null)
            {
                this.WriteSite(fabricResponse.Fabric);
                found = true;
            }

            if (!found)
            {
                throw new InvalidOperationException(
                    string.Format(
                    Properties.Resources.ServerNotFound,
                    this.Name,
                    PSRecoveryServicesClient.asrVaultCreds.ResourceName));
            }
        }

        /// <summary>
        /// Queries all / by default.
        /// </summary>
        private void GetAll()
        {
            FabricListResponse fabricListResponse =
                RecoveryServicesClient.GetAzureSiteRecoveryFabric();

            foreach (Fabric fabric in fabricListResponse.Fabrics)
            {
                // Do not process for fabrictype other than HyperVSite 
                if (String.Compare(fabric.Properties.CustomDetails.InstanceType, Constants.HyperVSite) != 0)
                    continue;

                this.WriteSite(fabric);
            }
        }

        /// <summary>
        /// Write Powershell Site.
        /// </summary>
        /// <param name="server">Fabric object</param>
        private void WriteSite(Fabric fabric)
        {
            this.WriteObject(new ASRSite(fabric));
        }       
    }
}
