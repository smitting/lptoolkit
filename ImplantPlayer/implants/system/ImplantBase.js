function ImplantInitializer(implant) { %%script%%
};

LPToolKit.registerImplant('%%typeid%%', ImplantInitializer);
/*

!!!! CAUTION: THIS IS NOT INTENDED TO BE USER EDITABLE !!!!

Project:    LPToolKit.Implants
File:       ImplantBase.js
Author:     Scott Mitting
Date:       2014-04-23
Abstract:

This is the script injected to the beginning of all implant scripts
to setup the plugin instance, so that plugin specific settings can
be passed to each implant without requiring a separate javascript
engine for each plugin.

*/



