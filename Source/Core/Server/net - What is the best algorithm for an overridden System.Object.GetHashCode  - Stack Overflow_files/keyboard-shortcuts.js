StackExchange.keyboardShortcuts=function(){function e(e,t,n){var i=$.cache[document[jQuery.expando]].events[e];if(i){for(var o,a=0;a<i.length;a++){var s=i[a];if(n==s.selector&&t.test(s.handler.toString())){o=s.handler;break}}o&&(n?$(document).off(e,n,o):$(document).off(e,o))}}function t(){e("keydown",/keyHappening = true/),e("keyup",/didn't generate a keypress event/),e("keypress",/resetModeIfNotApplicable/),e("click",/resetMode\(\);\s+reset\(\);/),e("click",/init\(state\.getSelectable\(\)\)/,".new-post-activity"),e("ajaxComplete",/ajaxNeedsReinit[\s\S]*ajaxNeedsNoReset/)}function n(){function e(e,t){var n="se-keyboard-shortcuts.settings.";if(arguments.length<2)try{return t=localStorage.getItem(n+e),"true"===t?!0:"false"===t?!1:t}catch(i){return}else try{return localStorage.setItem(n+e,t)}catch(i){return}}function t(e){var t=$(".keyboard-console pre");return e.length?(t.length||(t=$("<div class='keyboard-console'><pre /></div>").appendTo("body").find("pre")),e=e.replace(/^!(.*)$/gm,"<b>$1</b>"),t.html(e).parent().show(),void 0):(t.parent().hide(),void 0)}function n(){this.order=[],this.actions={}}function i(e){return e=$.trim(e.replace(/[\r\n ]+/g," ")),e.length>40&&(e=e.substr(0,37)+"..."),e}function o(e){var t=new n;return $(e+" > a").each(function(e,n){var i=$(n).text().replace(/^\s*-?[\d*]*\s*|\s*$/g,""),o=v[i];if(!o){if(o=i.replace(/[^a-z]/gi,"").toUpperCase(),!o){var a=/[\?&](answertab|tab|sort)=(\w+)/i.exec($(n).attr("href"))||[];o=(a[2]||"").toUpperCase()}for(;o.length&&t.actions[o.charAt(0)];)o=o.substr(1);if(!o.length)return StackExchange.debug.log("no suitable shortcut for sort order "+i),void 0}t.add(o.charAt(0),i,{"clickOrLink":n})}),t}function a(e,t,o){return{"name":e,"isApplicable":function(){return g.find(t).length},"getShortcuts":function(){var e,a,s,r,c,l=g.find(t+" "+o),u=Math.min(l.length,10);for(c=new n,e=0;u>e;e++){r=l.eq(e);var d=r.closest("li");a=i(d.find(".item-summary").text()),a.length||(a=i(d.find(".item-location").text())),a.length||(a=i(d.text())),s=r.attr("href");var h=9>e?""+(e+1):"0";c.add(h,a,{"url":s})}return c}}}function s(e,t,n,i){return{"onlyIf":".topbar "+e,"func":function(){g.find(e).click(),l(0)},"initiatesMode":a(t,n,i)}}function r(){return b&&b.isApplicable()?b:void 0}function c(i){function a(){var t="disableAutoHelp",n=e(t);e(t,!n),n=e(t),A.actions.H.name=n?"enable auto help":"disable auto help",D=StackExchange.helpers.DelayedReaction(C,n?2e3:5e3,{"sliding":!0})}function c(){var t=new n;A.add("G","go to",{"next":t}),q&&(A.add("U",q.firstText,{"func":function(){v(0,!1,!1,!1,!0)}}),A.add("J",q.nextText,{"func":function(){v(1,!1)}}),A.add("K",q.prevText,{"func":function(){v(-1,!1)}}),_.isQuestionPage||A.add("Enter",q.gotoText,{"clickOrLink":".keyboard-selected a"+(q.hrefOnly?"[href]":"")})),t.add("H","home page",{"url":"/"}),t.add("Q","questions",{"url":"/questions"}),t.add("T","tags",{"url":"/tags"}),t.add("U","users",{"url":"/users"}),t.add("B","badges",{"url":"/badges"}),t.add("N","unanswered",{"url":"/unanswered"}),t.add("A","ask question",{"link":"#nav-askquestion[href]"}),t.add("P","my profile",{"link":".profile-link,.profile-me"}),_.mainSiteLink?t.add("M","main site",{"url":_.mainSiteLink}):_.metaSiteLink&&t.add("M","meta site",{"url":_.metaSiteLink}),t.add("C","chat",{"link":"#footer-menu a:contains('chat')"});var i=new n;i.add("R","review",{"link":".topbar-menu-links a[href='/review/']"}),i.add("F","flags",{"link":".topbar .mod-only .icon-flag"}),t.add("S","special pages",{"next":i});var r=new n;if(_.isQuestionPage)g();else if(_.isProfilePage){r.add("T","tab",{"next":o("#tabs")}),r.add("U","user",{"next":o(".reloaded > .tabs")}),r.add("S","settings",{"next":o("#profile-side #side-menu li")}),_.isQuestionListing&&_.isAnswerListing&&(r.add("Q","questions",{"func":function(){d("questionListing")}}),r.add("A","answers",{"func":function(){d("answerListing")}}));var c=new n;c.add("M","moderation tools",{"click":".user-moderator-link","initiatesMode":w}),c.add("A","annotations",{"link":".mod-flag-indicator[href^='/users/history']"}),c.add("F","flagged posts",{"link":".mod-flag-indicator[href^='/users/flagged-posts']"}),A.add("M","moderate",{"next":c})}else(_.isQuestionListing||_.isTagInfoPage)&&A.add("O","order questions by",{"next":o("#tabs")});A.add("N","in-page navigation",{"next":r}),t.add("F","faq",{"url":"/faq"}),A.add("I","inbox",s(".js-inbox-button","Inbox item...",".inbox-dialog","li.inbox-item a")),A.add("R","recent achievements",s(".js-achievements-button","Achievement...",".achievements-dialog","ul.js-items li a")),A.add("Q","mod messages",s(".js-mod-inbox-button","Mod message...",".modInbox-dialog","li.inbox-item a")),A.add("F","show freshly updated data",{"click":".new-post-activity, #new-answer-activity","reinit":!0}),A.add("S","search",{"func":function(){$("#search input").focus()}});var l=u();A.add("P","page",{"next":l}),A.add("?","help",{"func":function(){if($(".keyboard-console").is(":visible"))return C(),void 0;var e="Keyboard shortcuts:";p&&(e=p+"\n"+e),x(e)},"noReset":!0,"unimportant":!0}),A.add("H",e("disableAutoHelp")?"enable auto help":"disable auto help",{"func":a,"unimportant":!0})}function u(){var e;_.isQuestionPage?e=".pager-answers":(e="",_.isQuestionListing&&_.isAnswerListing&&(e={"question":"#questions-table ","answer":"#answers-table "}[q.name]||""),e+=".pager:first");var t=new n;return t.add("F","first page",{"clickOrLink":e+" a[title='go to page 1']"}),t.add("P","previous page",{"clickOrLink":e+" a[rel='prev']"}),t.add("N","next page",{"clickOrLink":e+" a[rel='next']"}),t.add("L","last page",{"clickOrLink":e+" .current ~ a:not([rel='next']):last"}),t}function d(e){var t=O[e];t!==q&&(q=t,t.elements=$(t.selector),$(".keyboard-selected").removeClass("keyboard-selected"),A.actions.P.next=u(),l(t.elements.first().offset().top-100))}function h(){var e=$(".keyboard-selected");return e.is(".question")?parseInt(location.pathname.replace(/^\/questions\/(\d+)\/.*$/,"$1")):e.is(".answer")?parseInt(e.attr("id").replace("answer-","")):null}function g(){var e=new n;A.add("V","vote",{"next":e,"autoSelect":!0}),e.add("U","up",{"click":".keyboard-selected .vote-up-off"}),e.add("D","down",{"click":".keyboard-selected .vote-down-off"}),A.add("A","answer",{"func":function(){var e=$("#wmd-input:visible");e.length?e.focus():($("#show-editor-button input").click(),setTimeout(function(){$("#wmd-input:visible").focus()},0))},"onlyIf":"#wmd-input"}),$(".edit-post").length?A.add("E","edit",{"click":".keyboard-selected .edit-post","autoSelect":!0}):A.add("E","edit",{"link":".keyboard-selected .post-menu a[href$='/edit']","autoSelect":!0}),$("#edit-tags").length?A.add("T","retag",{"click":"#edit-tags"}):A.add("T","retag",{"link":".question .post-menu a[href$='?tagsonly=true']"}),A.add("C","add/show comments",{"click":".keyboard-selected .comments-link","autoSelect":!0}),A.add("L","link",{"click":".keyboard-selected .post-menu a[id^='link-post-']","autoSelect":!0});var t=new n;A.add("M","moderate",{"next":t,"autoSelect":!0}),t.add("F","flag",{"click":".keyboard-selected a[id^='flag-post-'], .keyboard-selected .flag-post-link","initiatesMode":w}),t.add("C","close",{"click":".keyboard-selected a[id^='close-question-'], .keyboard-selected .close-question-link","initiatesMode":w}),t.add("D","delete",{"click":".keyboard-selected a[id^='delete-post-']"}),t.add("E","suggested edit",{"click":".keyboard-selected a[id^='edit-pending-']"}),t.add("M","moderation tools",{"click":".keyboard-selected a.post-moderator-link","initiatesMode":w}),t.add("I","post issues",{"getNext":function(){return o(".keyboard-selected .post-issue-display")},"onlyIf":".keyboard-selected .post-issue-display"}),A.actions.G.next.add("O","post owner's profile",{"link":".keyboard-selected .post-signature:last .user-details a[href^='/users/']"}),A.actions.G.next.add("R","post revisions",{"func":function(e){I("/posts/"+h()+"/revisions",e.shiftKey)},"onlyIf":".keyboard-selected"}),A.add("O","order answers by",{"next":o("#tabs")})}function m(e){var t,n;if(e.hasOwnProperty("onlyIf")?t=e.onlyIf:(e.autoSelect&&!$(".keyboard-selected").length&&(v(1,!1,!0,!0),setTimeout(function(){$(".keyboard-selected").removeClass("keyboard-selected")},0)),t=e.clickOrLink?function(){return $(e.clickOrLink).length}:e.link||e.click),t&&(n=t,"string"==typeof t?t=function(){return $(n).length}:"function"!=typeof t&&(t=function(){return n})),t&&!t())return!1;var i;if(e.getNext?i=e.getNext():e.next&&(i=e.next),i){for(var o=0;o<i.order.length;o++)if(m(i.actions[i.order[o]]))return!0;return!1}return!0}function v(e,t,n,i,o){if(q&&(q.elements||(q.elements=$(q.selector)),q.elements.length)){var a,s,r,c,u,d,h,f,p,g,m,v=$(window),b=v.scrollTop(),w=v.height(),x=b+w,k=$(".keyboard-selected"),y=t;if(!(k.length&&n||((t||!k.length&&!o)&&(a=q.elements.filter(function(){var e=$(this),t=e.offset().top,n=e.height(),i=t+n,o=Math.max(0,Math.min(i,x)-Math.max(t,b));return o>=50?!0:o/n>=.5?!0:(b>t?d=e:i>x&&!h&&(h=e),!1)})),s=t?a:q.elements,o?u=s.eq(e):k.length?(r=s.index(k),-1===r&&0>e&&(r=0),c=y?(r+e+s.length)%s.length:Math.max(0,Math.min(r+e,s.length-1)),u=s.eq(c)):a.length?u=0>e?a.last():a.first():t||(u=0>e?d||h:h||d),!u||!u.length||(k.removeClass("keyboard-selected"),u.addClass("keyboard-selected"),i||t)))){if(p=u.offset().top,g=u.height(),p>=b&&x>p+g)return;if(g>w)return l(p),void 0;f=Math.max(.9,g/w),m=0>e?p+g-f*w:p-(1-f)*w,l(m)}}}function x(e){for(var n,i,o=e+"\n",a=!1,s=$(".keyboard-selected").length,r=0;r<P.order.length;r++)n=P.order[r],i=P.actions[n],m(i)&&(o+=(i.unimportant?"":"!")+(i.indent?"    ":"")+"<kbd>"+n+"</kbd> "+i.name,!s&&i.autoSelect&&(o+="*",a=!0),i.next&&(o+="..."),o+="\n");P.order.length||(o+="(no shortcuts available)"),a&&(o+="*auto-selects if nothing is selected"),t(o)}function k(e){e.each(function(){var e=$(this),t=e.queue("fx");t&&t.length&&e.queue("fx",function(e){setTimeout(C,0),e()})})}function y(){b=null}function S(){b&&!b.isApplicable()&&y()}function E(){P=A,t(""),D.cancel()}function C(){var t=r();return t?(P=t.getShortcuts(),e("disableAutoHelp")||x(t.name),D.cancel(),P.animated&&k(P.animated),void 0):(E(),void 0)}function T(e){return 13===e?"Enter":String.fromCharCode(e).toUpperCase()}function I(e,t){t?window.open(e):location.href=e}function M(t){if(t.ctrlKey||t.altKey||t.metaKey)return U.notHandled;if($(t.target).is("textarea, input[type='text'], input[type='url'], input[type='email'], input[type='password'], input:not([type])"))return U.notHandled;var n=P.actions[T(t.which)];if(!n)return U.notHandled;var i=n.reinit?U.handledReinitNow:n.noReset?U.handledNoReset:n.next||n.getNext?U.handled:U.handledResetNow;if(n.autoSelect&&v(1,!0,!0),!m(n))return U.notHandled;n.initiatesMode&&(b=n.initiatesMode);var o=n.url||$(n.link).attr("href");if(o)return I(o,t.shiftKey),i;if(n.click)return $(n.click).click(),i;if(n.clickOrLink){var a=$(n.clickOrLink),s=a.data("events"),r=!1;if(s&&s.click&&s.click.length)r=!0;else if(s=$(document).data("events"),s&&s.click)for(var c=0;c<s.click.length;c++){var l=s.click[c].selector;if(l&&a.is(l)){r=!0;break}}return r?a.click():I(a.attr("href"),t.shiftKey),i}var u;if(u=n.getNext?n.getNext():n.next){var d=n.name+"...";return P=u,e("disableAutoHelp")||x(d),i}return n.func?(n.func(t),i):(StackExchange.debug.log("action found, but nothing to do"),void 0)}var q,A=new n,P=A,_={},O={"questionPage":{"name":"post","selector":".question, .answer","firstText":"select question","nextText":"select next post","prevText":"select prev post"},"questionListing":{"name":"question","selector":"#questions .question-summary:visible, #question-mini-list .question-summary:visible, .user-questions .question-summary, #questions-table .question-summary, .fav-post .question-summary, #bounties-table .question-summary","firstText":"select first question","nextText":"select next question","prevText":"select prev question","gotoText":"go to selected question"},"answerListing":{"name":"answer","selector":"#answers-table .answer-summary .answer-link, .user-answers .answer-summary","firstText":"select first answer","nextText":"select next answer","prevText":"select prev answer","gotoText":"go to selected answer"},"tagListing":{"name":"tag","selector":"#tags-browser .tag-cell","firstText":"select first tag","nextText":"select next tag","prevText":"select prev tag","gotoText":"go to selected tag"},"userListing":{"name":"user","selector":"#user-browser .user-info","firstText":"select first user","nextText":"select next user","prevText":"select prev user","gotoText":"go to selected user"},"badgeListing":{"name":"badge","selector":"body.badges-page tr td:nth-child(2)","firstText":"select first badge","nextText":"select next badge","prevText":"select prev badge","gotoText":"go to selected badge"},"userSiteListing":{"name":"site","selector":"#content .module .account-container","firstText":"select first site","nextText":"select next site","prevText":"select prev site","gotoText":"go to selected site"},"activityListing":{"name":"activity","selector":"table.history-table tr:has(div.date) td:last-child","firstText":"select first activity","nextText":"select next activity","prevText":"select prev activity","gotoText":"go to selected activity"},"reputationListing":{"name":"rep","selector":"table.rep-table > tbody > tr.rep-table-row > td:last-child","firstText":"select first rep","nextText":"select next rep","prevText":"select prev rep","gotoText":"go to selected rep"},"flagListing":{"name":"flag","selector":"table.flagged-posts.moderator > tbody > tr > td div.mod-post-header","hrefOnly":!0,"firstText":"select first flag","nextText":"select next flag","prevText":"select prev flag","gotoText":"go to selected flag"}};if(/^\/questions\/\d+/i.test(location.pathname)?(_.isQuestionPage=!0,q=O.questionPage):/^\/users(\/(edit|preferences|apps|mylogins|hidecommunities))?\/(\d)+/i.test(location.pathname)?_.isProfilePage=!0:/^\/tags\/[^\/]+\/info$/i.test(location.pathname)&&(_.isTagInfoPage=!0),$(O.answerListing.selector).length&&(_.isAnswerListing=!0,q=O.answerListing),$(O.questionListing.selector).length)_.isQuestionListing=!0,q=O.questionListing;else for(var j in O)if(O.hasOwnProperty(j)&&!/(?:question|answer)Listing/.test(j)&&$(O[j].selector).length){q=O[j],_["is"+j.charAt(0).toUpperCase()+j.substr(1)]=!0;break}i&&(q=i),q&&(q.elements=$(q.selector));var L=location.hostname;if(/^meta\./.test(L)||/\.meta\.stackexchange\.com$/.test(L))_.mainSiteLink="//"+L.replace(/(^|\.)meta./,"$1");else{var R=$("#footer-menu .top-footer-links a:last").attr("href");R.replace(/^.*\/\//,"").replace(/(^|\.)meta./,"$1")==L&&(_.metaSiteLink=R)}var D=StackExchange.helpers.DelayedReaction(C,e("disableAutoHelp")?2e3:5e3,{"sliding":!0}),U={"notHandled":0,"handled":1,"handledNoReset":2,"handledResetNow":3,"handledReinitNow":4},N=!1,B=function(){N=!0},H=function(e){27===e.which?(y(),E()):N&&C(),N=!1},F=function(e){N=!1;var t=M(e);switch(t){case U.notHandled:return S(),C(),!0;case U.handled:return D.trigger(),!1;case U.handledResetNow:return C(),!1;case U.handledNoReset:return D.cancel(),!1;case U.handledReinitNow:return f(),!1}},V=function(e){"undefined"!=typeof e.which&&(y(),C())};return $(document).keydown(B),$(document).keyup(H),$(document).keypress(F),$(document).click(V),c(),C(),{"getSelectable":function(){return q},"cancel":function(){$(document).unbind("keydown",B),$(document).unbind("keyup",H),$(document).unbind("keypress",F),$(document).unbind("click",V)},"reset":C}}function l(e){var t=$(window);x.pos=t.scrollTop(),$(x).stop().animate({"pos":e},{"duration":200,"step":function(){t.scrollTop(this.pos)},"complete":function(){t.scrollTop(e)}})}function u(e){var t,n=[],i=!1;for(t=0;t<k.length;t++){var o=k[t];o.re.test(e)?(o.deferred.resolve(),i=!0):n.push(o)}return k=n,i}function d(e){return/users\/stats\/(questions|answers)|posts\/\d+\/ajax-load-mini|posts\/ajax-load-realtime/.test(e)}function h(e){return/mini-profiler-results|users\/login\/global|\.js/.test(e)}function f(){y.cancel(),y=c(y.getSelectable())}var p;if(window.StackExchange&&StackExchange.helpers&&StackExchange.helpers.DelayedReaction){var g=$("body > .topbar"),m=".keyboard-console { background-color: black; background-color: rgba(0, 0, 0, .8); position: fixed; left: 100px; bottom: 100px;padding: 10px; text-align: left; border-radius: 6px; z-index: 1000 }.keyboard-console pre { background-color: transparent; color: #ccc; width: auto; height: auto; padding: 0; margin: 0; overflow: visible; line-height:1.5; border: none;}.keyboard-console pre b, .keyboard-console pre a { color: white !important; }.keyboard-console pre kbd { display: inline-block; font-family: monospace; }.keyboard-selected { box-shadow: 15px 15px 50px rgba(0, 0, 0, .2) inset; }";$("<style type='text/css' />").text(m).appendTo("head"),n.prototype.add=function(e,t,n){this.actions[e]&&StackExchange.debug.log("duplicate shortcut "+e),this.order.push(e),this.actions[e]=n,n.name=$("<span />").text(t).html()};var v={};v.featured="B",v.bugs="G";var b,w={"name":"Popup...","isApplicable":function(){return $(".popup").length},"getShortcuts":function(){var e=$(".popup-active-pane"),t=new n,a=1,s=65,r=[],c=$(".popup .subheader [id='tabs']");c.find("> a").length>1&&t.add("T","switch tab",{"next":o(".popup .subheader [id='tabs']")}),e.length||(e=$(".popup"));var l=e.find(".action-list > li > label > input[type='radio']:visible, .action-list > li > a[href]:visible, .action-list > li > .migration-targets input[type='radio']:visible");return l.length>10&&(s+=l.length-10),l.each(function(){var e,n=$(this),o=n.closest("li"),c=o.add(o.find("> .migration-targets td:not(.target-icon)")).find("> label .action-name"),l=o.find(".action-subform");return e=10>a?""+a:10==a?"0":String.fromCharCode(54+a),n.is("a")?(t.add(e,i(n.text()),{"link":n}),a++,void 0):(t.add(e,i(c.text())||"unknown action",{"func":function(){n.focus().click()}}),l.length&&(l.find("input[type='radio']:visible").each(function(){var e=$(this),n=e.parent().find(".action-name");n.length||(n=e.parent()),t.add(String.fromCharCode(s),i(n.text()||"other"),{"func":function(){e.focus().click()},"indent":!0}),s++}),r.push(l)),a++,void 0)}),r.length&&(t.animated=$(r)),t}},x={},k=[],y=c();$(document).ajaxComplete(function(e,t,n){u(n.url)||(d(n.url)?f():h(n.url)||y.reset())}),$(document).on("click",".new-post-activity",f)}}var i=!1;return{"init":function(e){i||(i=!0,e&&t(),n())}}}();