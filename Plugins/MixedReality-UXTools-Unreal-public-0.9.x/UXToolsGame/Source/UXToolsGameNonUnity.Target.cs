// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using UnrealBuildTool;
using System.Collections.Generic;

public class UXToolsGameNonUnityTarget : TargetRules
{
	public UXToolsGameNonUnityTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Game;
		DefaultBuildSettings = BuildSettingsVersion.V2;

		bUseUnityBuild = false;

		ExtraModuleNames.AddRange( new string[] { "UXToolsGame", "UXToolsTests" } );
	}
}

