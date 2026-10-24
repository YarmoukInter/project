// Excel-compatible UTF-8 CSV export shared by coordinator record pages.
(() => {
  'use strict';
  function safeCell(value) {
    let text = String(value == null ? '' : value).replace(/\r\n?/g, '\n');
    if (/^[\s\u0000-\u001f]*[=+@\-]/.test(text)) text = "'" + text;
    return '"' + text.replace(/"/g, '""') + '"';
  }
  function download(filename, headers, rows) {
    const content = '\ufeff' + [headers, ...rows].map(row => row.map(safeCell).join(',')).join('\r\n');
    const blob = new Blob([content], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.hidden = true;
    document.body.append(link);
    link.click();
    link.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }
  window.UimsExcelExport = { download };
})();
