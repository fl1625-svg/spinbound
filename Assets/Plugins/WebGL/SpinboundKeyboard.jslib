mergeInto(LibraryManager.library, {
  $SpinboundWebKeyboard: {
    initialized: false,
    downMask: 0,
    pressedMask: 0,
    codeBits: {
      KeyW: 1 << 0,
      KeyA: 1 << 1,
      KeyS: 1 << 2,
      KeyD: 1 << 3,
      ArrowUp: 1 << 4,
      ArrowLeft: 1 << 5,
      ArrowDown: 1 << 6,
      ArrowRight: 1 << 7,
      ShiftLeft: 1 << 8,
      ShiftRight: 1 << 8,
      Space: 1 << 9,
      KeyR: 1 << 10,
      Escape: 1 << 11
    },
    init: function () {
      if (SpinboundWebKeyboard.initialized) return;
      SpinboundWebKeyboard.initialized = true;

      var tracked = SpinboundWebKeyboard.codeBits;
      var shouldPreventDefault = function (code) {
        return code === 'ArrowUp' || code === 'ArrowLeft' || code === 'ArrowDown' ||
          code === 'ArrowRight' || code === 'Space';
      };

      window.addEventListener('keydown', function (event) {
        var bit = tracked[event.code];
        if (bit === undefined) return;
        if ((SpinboundWebKeyboard.downMask & bit) === 0) {
          SpinboundWebKeyboard.pressedMask |= bit;
        }
        SpinboundWebKeyboard.downMask |= bit;
        if (shouldPreventDefault(event.code)) event.preventDefault();
      }, true);

      window.addEventListener('keyup', function (event) {
        var bit = tracked[event.code];
        if (bit === undefined) return;
        SpinboundWebKeyboard.downMask &= ~bit;
        if (shouldPreventDefault(event.code)) event.preventDefault();
      }, true);

      window.addEventListener('blur', function () {
        SpinboundWebKeyboard.downMask = 0;
        SpinboundWebKeyboard.pressedMask = 0;
      });
    }
  },

  Spinbound_GetKeyboardDownMask__deps: ['$SpinboundWebKeyboard'],
  Spinbound_GetKeyboardDownMask: function () {
    SpinboundWebKeyboard.init();
    return SpinboundWebKeyboard.downMask | 0;
  },

  Spinbound_ConsumeKeyboardPressedMask__deps: ['$SpinboundWebKeyboard'],
  Spinbound_ConsumeKeyboardPressedMask: function () {
    SpinboundWebKeyboard.init();
    var result = SpinboundWebKeyboard.pressedMask | 0;
    SpinboundWebKeyboard.pressedMask = 0;
    return result;
  }
});
