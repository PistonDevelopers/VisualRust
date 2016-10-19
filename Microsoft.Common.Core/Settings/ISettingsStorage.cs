// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;

namespace Microsoft.Common.Core.Settings {
    /// <summary>
    /// Settings storage. Exported via MEF for a particular content type.
    /// Editor uses exported object to retrieve its settings such as indentation
    /// style, tab size, formatting options and so on.
    /// </summary>
    public interface ISettingsStorage {
        event EventHandler<EventArgs> SettingsChanged;

        void LoadFromStorage();

        string GetString(string name, string defaultValue);
        int GetInteger(string name, int defaultValue);
        bool GetBoolean(string name, bool defaultValue);
    }

    /// <summary>
    /// Writable settings storage. Exported via MEF for a particular content type.
    /// Editor uses exported object to store settings such as indentation style, 
    /// tab size, formatting options and so on.
    /// </summary>
    public interface IWritableSettingsStorage : ISettingsStorage {
        void ResetSettings();

        void SetString(string name, string value);
        void SetInteger(string name, int value);
        void SetBoolean(string name, bool value);

        /// <summary>
        /// Tells storage that multiple options are about to be changed. Storage should stop 
        /// firing events on every settings change and instead postpone even firing until 
        /// <see cref="EndBatchChange"/> is called.
        /// </summary>
        void BeginBatchChange();

        /// <summary>
        /// Ends multiple settings change. Storage will file a single 'settings changed' event.
        /// </summary>
        void EndBatchChange();
    }
}
