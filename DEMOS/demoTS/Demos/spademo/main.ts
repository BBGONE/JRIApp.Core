﻿import * as RIAPP from "jriapp";

import { IMainOptions, DemoApplication } from "./app";
import * as COMMON from "common";
import * as AUTOCOMPLETE from "autocomplete";
import * as GRIdELVIEW from "./gridElView";
import * as PRODAUTOCOMPLETE from "./prodAutocomplete";


//bootstrap error handler - the last resort (typically display message to the user)
RIAPP.bootstrapper.objEvents.addOnError(function (_s, args) {
    debugger;
    alert(args.error.message);
});

export function start(options: IMainOptions) {
    options.modulesInits = {
        "COMMON": COMMON.initModule,
        "AUTOCOMPLETE": AUTOCOMPLETE.initModule,
        "GRIdELVIEW": GRIdELVIEW.initModule,
        "PRODAUTOCOMPLETE": PRODAUTOCOMPLETE.initModule
    };

    //create and start application here
    return RIAPP.bootstrapper.startApp(() => {
        return new DemoApplication(options);
    }, (app) => {
        // loads templates in groups (on demand) - the whole group of templates is loaded
        // when a single template from the group is needed
        app.registerTemplateGroup('custGroup', options.spa_template1_url);

        app.registerTemplateGroup('custInfoGroup', options.spa_template2_url);

        app.registerTemplateGroup('custAdrGroup', options.spa_template3_url);
            
        }).then((app) => {
            return app.customerVM.load();
        });
}