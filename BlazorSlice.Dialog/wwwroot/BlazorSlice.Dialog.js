class ElementReference {
    constructor() {
        this.listenerId = 0;
        this.eventListeners = {};
    }

    focus(element) {
        if (element) {
            element.focus();
        }
    }

    blur(element) {
        if (element) {
            element.blur();
        }
    }

    focusFirst(element, skip = 0, min = 0) {
        if (element) {
            let tabbables = getTabbableElements(element);
            if (tabbables.length <= min)
                element.focus();
            else
                tabbables[skip].focus();
        }
    }

    focusLast(element, skip = 0, min = 0) {
        if (element) {
            let tabbables = getTabbableElements(element);
            if (tabbables.length <= min)
                element.focus();
            else
                tabbables[tabbables.length - skip - 1].focus();
        }
    }

    saveFocus(element) {
        if (element) {
            element['blazorSlice_savedFocus'] = document.activeElement;
        }
    }

    restoreFocus(element) {
        if (element) {
            let previous = element['blazorSlice_savedFocus'];
            delete element['blazorSlice_savedFocus']
            if (previous)
                previous.focus();
        }
    }

    selectRange(element, pos1, pos2) {
        if (element) {
            if (element.createTextRange) {
                let selRange = element.createTextRange();
                selRange.collapse(true);
                selRange.moveStart('character', pos1);
                selRange.moveEnd('character', pos2);
                selRange.select();
            } else if (element.setSelectionRange) {
                element.setSelectionRange(pos1, pos2);
            } else if (element.selectionStart) {
                element.selectionStart = pos1;
                element.selectionEnd = pos2;
            }
            element.focus();
        }
    }

    select(element) {
        if (element) {
            element.select();
        }
    }

    getClientRectFromParent(element) {
        if (!element) return;
        let parent = element.parentElement;
        if (!parent) return;
        return this.getBoundingClientRect(parent);
    }

    getClientRectFromFirstChild(element) {
        if (!element) return;
        let child = element.children && element.children[0];
        if (!child) return;
        return this.getBoundingClientRect(child);
    }

    getBoundingClientRect(element) {
        if (!element) return;

        let rect = JSON.parse(JSON.stringify(element.getBoundingClientRect()));

        rect.scrollY = window.scrollY || document.documentElement.scrollTop;
        rect.scrollX = window.scrollX || document.documentElement.scrollLeft;

        rect.windowHeight = window.innerHeight;
        rect.windowWidth = window.innerWidth;
        return rect;
    }

    hasFixedAncestors(element) {
        for (; element && element !== document; element = element.parentNode) {
            if (window.getComputedStyle(element).getPropertyValue("position") === "fixed")
                return true;
        }
        return false;
    }

    changeCss(element, css) {
        if (element) {
            element.className = css;
        }
    }

    changeCssVariable(element, name, newValue) {
        if (element) {
            element.style.setProperty(name, newValue);
        }
    }

    addEventListener(element, dotnet, event, callback, spec, stopPropagation) {
        let listener = function (e) {
            const args = Array.from(spec, x => serializeParameter(e, x));
            dotnet.invokeMethodAsync(callback, ...args);
            if (stopPropagation) {
                e.stopPropagation();
            }
        };
        element.addEventListener(event, listener);
        this.eventListeners[++this.listenerId] = listener;
        return this.listenerId;
    }

    removeEventListener(element, event, eventId) {
        element.removeEventListener(event, this.eventListeners[eventId]);
        delete this.eventListeners[eventId];
    }

    addDefaultPreventingHandler(element, eventName) {
        let listener = function (e) {
            e.preventDefault();
        }
        element.addEventListener(eventName, listener, {passive: false});
        this.eventListeners[++this.listenerId] = listener;
        return this.listenerId;
    }

    removeDefaultPreventingHandler(element, eventName, listenerId) {
        this.removeEventListener(element, eventName, listenerId);
    }

    addDefaultPreventingHandlers(element, eventNames) {
        let listeners = [];

        for (const eventName of eventNames) {
            let listenerId = this.addDefaultPreventingHandler(element, eventName);
            listeners.push(listenerId);
        }

        return listeners;
    }

    removeDefaultPreventingHandlers(element, eventNames, listenerIds) {
        for (let index = 0; index < eventNames.length; ++index) {
            const eventName = eventNames[index];
            const listenerId = listenerIds[index];
            this.removeDefaultPreventingHandler(element, eventName, listenerId);
        }
    }
}
window.elementRef = new ElementReference();

class KeyInterceptorFactory {

    connect(dotNetRef, elementId, options) {
        //console.log('[ArcBlazor | ArcKeyInterceptorFactory] connect ', { dotNetRef, element, options });
        if (!elementId)
            throw "elementId: expected element id!";
        var element = document.getElementById(elementId);
        if (!element)
            throw "no element found for id: " +elementId;
        if (!element.keyInterceptor)
            element.keyInterceptor = new KeyInterceptor(dotNetRef, options);
        element.keyInterceptor.connect(element);
    }

    updatekey(elementId, option) {
        var element = document.getElementById(elementId);
        if (!element || !element.keyInterceptor)
            return;
        element.keyInterceptor.updatekey(option);
    }

    disconnect(elementId) {
        var element = document.getElementById(elementId);
        if (!element || !element.keyInterceptor)
            return;
        element.keyInterceptor.disconnect();
    }
}
window.keyInterceptor = new KeyInterceptorFactory();


class KeyInterceptor {

    constructor(dotNetRef, options) {
        this._dotNetRef = dotNetRef;
        this._options = options;
        this.logger = options.enableLogging ? console.log : (message) => { };
        this.logger('[ArcBlazor | KeyInterceptor] Interceptor initialized', { options });
    }


    connect(element) {
        if (!this._options)
            return;
        if (!this._options.keys)
            throw "_options.keys: array of KeyOptions expected";
        if (!this._options.targetClass)
            throw "_options.targetClass: css class name expected";
        if (this._observer) {
            // don't do double registration
            return;
        }
        var targetClass = this._options.targetClass;
        this.logger('[ArcBlazor | KeyInterceptor] Start observing DOM of element for changes to child with class ', { element, targetClass});
        this._element = element;
        this._observer = new MutationObserver(this.onDomChanged);
        this._observer.keyInterceptor = this;
        this._observer.observe(this._element, { attributes: false, childList: true, subtree: true });
        this._observedChildren = [];
        // transform key options into a key lookup
        this._keyOptions = {};
        this._regexOptions = [];
        for (const keyOption of this._options.keys) {
            if (!keyOption || !keyOption.key) {
                this.logger('[ArcBlazor | KeyInterceptor] got invalid key options: ', keyOption);
                continue;
            }
            this.setKeyOption(keyOption)
        }
        this.logger('[ArcBlazor | KeyInterceptor] key options: ', this._keyOptions);
        if (this._regexOptions.size > 0)
            this.logger('[ArcBlazor | KeyInterceptor] regex options: ', this._regexOptions);
        // register handlers
        for (const child of this._element.getElementsByClassName(targetClass)) {
            this.attachHandlers(child);
        }
    }

    setKeyOption(keyOption) {
        if (keyOption.key.length > 2 && keyOption.key.startsWith('/') && keyOption.key.endsWith('/')) {
            // JS regex key options such as "/[a-z]/" or "/a|b/" but NOT "/[a-z]/g" or "/[a-z]/i"
            keyOption.regex = new RegExp(keyOption.key.substring(1, keyOption.key.length - 1)); // strip the / from start and end
            this._regexOptions.push(keyOption);
        }
        else
            this._keyOptions[keyOption.key.toLowerCase()] = keyOption;
        // remove whitespace and enforce lowercase
        var whitespace = new RegExp("\\s", "g");
        keyOption.preventDown = (keyOption.preventDown || "none").replace(whitespace, "").toLowerCase();
        keyOption.preventUp = (keyOption.preventUp || "none").replace(whitespace, "").toLowerCase();
        keyOption.stopDown = (keyOption.stopDown || "none").replace(whitespace, "").toLowerCase();
        keyOption.stopUp = (keyOption.stopUp || "none").replace(whitespace, "").toLowerCase();
    }

    updatekey(updatedOption) {
        var option = this._keyOptions[updatedOption.key.toLowerCase()];
        option || this.logger('[ArcBlazor | KeyInterceptor] updating option failed: key not registered');
        this.setKeyOption(updatedOption);
        this.logger('[ArcBlazor | KeyInterceptor] updated option ', { option, updatedOption });
    }

    disconnect() {
        if (!this._observer)
            return;
        this.logger('[ArcBlazor | KeyInterceptor] disconnect mutation observer and event handlers');
        this._observer.disconnect();
        this._observer = null;
        for (const child of this._observedChildren)
            this.detachHandlers(child);
    }

    attachHandlers(child) {
        this.logger('[ArcBlazor | KeyInterceptor] attaching handlers ', { child });
        if (this._observedChildren.indexOf(child) > -1) {
            //console.log("... already attached");
            return;
        }
        child.keyInterceptor = this;
        child.addEventListener('keydown', this.onKeyDown);
        child.addEventListener('keyup', this.onKeyUp);
        this._observedChildren.push(child);
    }

    detachHandlers(child) {
        this.logger('[ArcBlazor | KeyInterceptor] detaching handlers ', { child });
        child.removeEventListener('keydown', this.onKeyDown);
        child.removeEventListener('keyup', this.onKeyUp);
        this._observedChildren = this._observedChildren.filter(x=>x!==child);
    }

    onDomChanged(mutationsList, observer) {
        var self = this.keyInterceptor; // func is invoked with this == _observer
        //self.logger('[ArcBlazor | KeyInterceptor] onDomChanged: ', { self });
        var targetClass = self._options.targetClass;
        for (const mutation of mutationsList) {
            //self.logger('[ArcBlazor | KeyInterceptor] Subtree mutation: ', { mutation });
            for (const element of mutation.addedNodes) {
                if (element.classList && element.classList.contains(targetClass))
                    self.attachHandlers(element);
            }
            for (const element of mutation.removedNodes) {
                if (element.classList && element.classList.contains(targetClass))
                    self.detachHandlers(element);
            }
        }
    }

    matchesKeyCombination(option, args) {
        if (!option || option=== "none")
            return false;
        if (option === "any")
            return true;
        var shift = args.shiftKey;
        var ctrl = args.ctrlKey;
        var alt = args.altKey;
        var meta = args.metaKey;
        var any = shift || ctrl || alt || meta;
        if (any && option === "key+any")
            return true;
        if (!any && option.includes("key+none"))
            return true;
        if (!any)
            return false;
        var combi = `key${shift ? "+shift" : ""}${ctrl ? "+ctrl" : ""}${alt ? "+alt" : ""}${meta ? "+meta" : ""}`;
        return option.includes(combi);
    }

    onKeyDown(args) {
        let self = this.keyInterceptor; // func is invoked with this == child
        let key = args.key.toLowerCase();
        self.logger('[ArcBlazor | KeyInterceptor] down "' + key + '"', args);
        let invoke = false;
        if (self._keyOptions.hasOwnProperty(key)) {
            var keyOptions = self._keyOptions[key];
            self.logger('[ArcBlazor | KeyInterceptor] options for "' + key + '"', keyOptions);
            self.processKeyDown(args, keyOptions);
            if (keyOptions.subscribeDown)
                invoke = true;
        }
        for (const keyOptions of self._regexOptions) {
            if (keyOptions.regex.test(key)) {
                self.logger('[ArcBlazor | KeyInterceptor] regex options for "' + key + '"', keyOptions);
                self.processKeyDown(args, keyOptions);
                if (keyOptions.subscribeDown)
                    invoke = true;
            }
        }
        if (invoke) {
            let eventArgs = self.toKeyboardEventArgs(args);
            eventArgs.Type = "keydown";
            // we'd like to pass a reference to the child element back to dotnet but we can't
            // https://github.com/dotnet/aspnetcore/issues/16110
            // if we ever need it we'll pass the id up and users need to id the observed elements
            self._dotNetRef.invokeMethodAsync('OnKeyDown', eventArgs);
        }
    }

    processKeyDown(args, keyOptions) {
        if (this.matchesKeyCombination(keyOptions.preventDown, args))
            args.preventDefault();
        if (this.matchesKeyCombination(keyOptions.stopDown, args))
            args.stopPropagation();
    }

    onKeyUp(args) {
        let self = this.keyInterceptor; // func is invoked with this == child
        let key = args.key.toLowerCase();
        self.logger('[ArcBlazor | KeyInterceptor] up "' + key + '"', args);
        let invoke = false;
        if (self._keyOptions.hasOwnProperty(key)) {
            let keyOptions = self._keyOptions[key];
            self.processKeyUp(args, keyOptions);
            if (keyOptions.subscribeUp)
                invoke = true;
        }
        for (const keyOptions of self._regexOptions) {
            if (keyOptions.regex.test(key)) {
                self.processKeyUp(args, keyOptions);
                if (keyOptions.subscribeUp)
                    invoke = true;
            }
        }
        if (invoke) {
            let eventArgs = self.toKeyboardEventArgs(args);
            eventArgs.Type = "keyup";
            // we'd like to pass a reference to the child element back to dotnet but we can't
            // https://github.com/dotnet/aspnetcore/issues/16110
            // if we ever need it we'll pass the id up and users need to id the observed elements
            self._dotNetRef.invokeMethodAsync('OnKeyUp', eventArgs);
        }
    }

    processKeyUp(args, keyOptions) {
        if (this.matchesKeyCombination(keyOptions.preventUp, args))
            args.preventDefault();
        if (this.matchesKeyCombination(keyOptions.stopUp, args))
            args.stopPropagation();
    }

    toKeyboardEventArgs(args) {
        return {
            Key: args.key,
            Code: args.code,
            Location: args.location,
            Repeat: args.repeat,
            CtrlKey: args.ctrlKey,
            ShiftKey: args.shiftKey,
            AltKey: args.altKey,
            MetaKey: args.metaKey
        };
    }

}
window.getTabbableElements = (element) => {
    return element.querySelectorAll(
        "a[href]:not([tabindex='-1'])," +
        "area[href]:not([tabindex='-1'])," +
        "button:not([disabled]):not([tabindex='-1'])," +
        "input:not([disabled]):not([tabindex='-1']):not([type='hidden'])," +
        "select:not([disabled]):not([tabindex='-1'])," +
        "textarea:not([disabled]):not([tabindex='-1'])," +
        "iframe:not([tabindex='-1'])," +
        "details:not([tabindex='-1'])," +
        "[tabindex]:not([tabindex='-1'])," +
        "[contentEditable=true]:not([tabindex='-1']"
    );
};