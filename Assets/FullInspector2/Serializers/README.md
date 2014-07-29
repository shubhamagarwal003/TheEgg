## Overview

This folder contains all of the serializers that Full Inspector ships with.

By default, Full Inspector will use Json.NET. You can change this by modifying `BaseBehavior` and `BaseScriptableObject` so that they derive from, for example, `BaseBehavior<ProtoBufNetSerializer>` and `BaseScriptableObject<ProtoBufNetSerializer>`, respectively.
