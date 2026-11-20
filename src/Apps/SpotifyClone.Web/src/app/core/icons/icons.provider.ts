import { EnvironmentProviders, inject, provideAppInitializer } from '@angular/core';
import { FaIconLibrary } from '@fortawesome/angular-fontawesome';
import { faFacebook, faGoogle, faInstagram, faTwitter } from '@fortawesome/free-brands-svg-icons';
import { faCircleLeft, faCircleRight } from '@fortawesome/free-regular-svg-icons';
import {
  faArrowDown,
  faBackwardStep,
  faBars,
  faBell,
  faBucket,
  faCaretDown,
  faCheck,
  faChevronUp,
  faCircleCheck,
  faClock,
  faComputer,
  faEllipsis,
  faExclamation,
  faExpand,
  faEye,
  faEyeSlash,
  faForwardStep,
  faHouse,
  faLessThan,
  faListUl,
  faMagnifyingGlass,
  faMicrophone,
  faPause,
  faPen,
  faPlay,
  faPlus,
  faRepeat,
  faShuffle,
  faUpRightAndDownLeftFromCenter,
  faUpRightFromSquare,
  faUpload,
  faUserGroup,
  faUserPlus,
  faVolumeHigh,
  faVolumeXmark,
  faXmark,
  faBookOpen,
} from '@fortawesome/free-solid-svg-icons';

/**
 * Every icon the UI uses, registered once so templates can reference them by name
 * (`<fa-icon [icon]="['fas', 'play']" />`) while the bundle only ships what is listed here.
 */
const APP_ICONS = [
  // brands
  faFacebook,
  faGoogle,
  faInstagram,
  faTwitter,
  // regular
  faCircleLeft,
  faCircleRight,
  // solid
  faArrowDown,
  faBackwardStep,
  faBars,
  faBell,
  faBookOpen,
  faBucket,
  faCaretDown,
  faCheck,
  faChevronUp,
  faCircleCheck,
  faClock,
  faComputer,
  faEllipsis,
  faExclamation,
  faExpand,
  faEye,
  faEyeSlash,
  faForwardStep,
  faHouse,
  faLessThan,
  faListUl,
  faMagnifyingGlass,
  faMicrophone,
  faPause,
  faPen,
  faPlay,
  faPlus,
  faRepeat,
  faShuffle,
  faUpRightAndDownLeftFromCenter,
  faUpRightFromSquare,
  faUpload,
  faUserGroup,
  faUserPlus,
  faVolumeHigh,
  faVolumeXmark,
  faXmark,
];

export function provideIcons(): EnvironmentProviders {
  return provideAppInitializer(() => {
    inject(FaIconLibrary).addIcons(...APP_ICONS);
  });
}
