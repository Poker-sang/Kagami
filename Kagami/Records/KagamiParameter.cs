﻿namespace Kagami.Records;

internal record KagamiParameter(Type Type, string Name, bool HasDefault, object? Default, string Description);