const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const test = require('node:test');

const read = file => fs.readFileSync(path.join(__dirname, '..', file), 'utf8');

test('affected tables select a content-sized action column without changing Members', () => {
    for (const file of ['Views/Event/Index.cshtml', 'Views/AdminAccounts/Index.cshtml']) {
        assert.match(read(file), /<th[^>]*class="actions actions-balanced"[^>]*>Actions<\/th>/);
    }
    for (const file of ['Views/AccountRequest/Index.cshtml', 'Views/CertificateLedger/Index.cshtml', 'Views/RegistrationReview/Index.cshtml']) {
        assert.match(read(file), /<th[^>]*class="[^"]*actions-decision[^"]*"[^>]*>Actions<\/th>/);
    }
    assert.doesNotMatch(read('Views/Member/Index.cshtml'), /actions-balanced|actions-decision/);
    const css = read('Styles/app.css');
    assert.match(css, /th\.actions-balanced\s*\{\s*width:\s*200px/);
    assert.match(css, /th\.actions-decision\s*\{\s*width:\s*232px/);
    assert.match(css, /@media \(min-width: 641px\) and \(max-width: 850px\)[\s\S]*?th\.actions-decision\s*\{\s*width:\s*30%/);
});

test('attendance and check-in rows retain responsive labels and aligned actions', () => {
    const attendance = read('Views/MyEvents/_AttendanceHistoryPartial.cshtml');
    assert.match(attendance, /<th scope="row" data-label="Event">/);
    assert.match(attendance, /data-label="Recorded" class="numeric cell-date"/);
    assert.match(attendance, /Reason: @row\.InvalidationReason/);
    const css = read('Styles/app.css');
    assert.match(css, /\.attendance-history \.data-table tbody th\[scope="row"\]\s*\{[^}]*text-transform:\s*none/s);
    assert.match(css, /\.attendance-history \.data-table tbody th\[scope="row"\]\s*\{[^}]*overflow-wrap:\s*anywhere/s);
    const checkin = read('Views/CheckIn/Index.cshtml');
    assert.match(checkin, /row\.IsInvalidated \? "No actions available" : "Checked in"/);
    for (const label of ['Participant', 'Student number', 'Attendance', 'Recorded', 'Outcome', 'Action']) {
        assert.ok(checkin.includes(`data-label="${label}"`));
    }
    assert.match(checkin, /result\.CanInvalidate/);
    assert.match(checkin, /<td data-label="Outcome">[\s\S]*?Reason: @result\.Message[\s\S]*?<td data-label="Action"/);
    const log = checkin.split('<tbody data-scan-log>')[1];
    const actionCell = log.match(/<td data-label="Action"[^>]*>([\s\S]*?)<\/td>/)[1];
    assert.doesNotMatch(actionCell, /@result\.Message/);
    assert.match(actionCell, /else if \(result\.IsInvalidated\)[\s\S]*?No actions available/);
    const scanner = read('wwwroot/js/features/scanner.js');
    assert.match(scanner, /data-dialog-open.*invalidate-dialog/);
    assert.match(scanner, /button button-quiet-danger/);
});

test('venue editor shares preview and standalone script patterns', () => {
    const form = read('Views/Venue/_Form.cshtml');
    assert.match(form, /data-record-preview-alt="Preview of selected venue photo"/);
    assert.match(form, /Current photo/);
    assert.match(form, /Choose the photo again/);
    for (const file of ['Views/Venue/Create.cshtml', 'Views/Venue/Edit.cshtml']) {
        assert.match(read(file), /js\/features\/record-dialog\.js/);
    }
});

test('mobile attendance row headers grow with wrapped event titles', () => {
    const css = read('Styles/features/my-events.css');
    assert.match(css, /@media\s*\(max-width:\s*640px\)\s*\{\s*\.my-events-panel \.attendance-history \.data-table tbody th\[scope="row"\]\s*\{[^}]*height:\s*auto\s*;/);
});

test('registration dates align with their headings and event type widths cover every table state', () => {
    const review = read('Views/RegistrationReview/Index.cshtml');
    assert.match(review, /data-label="Submission date" class="numeric cell-date"/);
    assert.match(review, /data-label="Submission time" class="numeric cell-date"/);
    const types = read('Views/EventType/Index.cshtml');
    assert.equal((types.match(/<table class="data-table table-sticky event-type-table">/g) || []).length, 3);
    const css = read('Styles/app.css');
    assert.match(css, /\.data-table \.numeric\.cell-date\s*\{\s*text-align:\s*left/);
    assert.match(css, /\.event-type-table thead th:first-child\s*\{\s*width:\s*30%/);
    assert.match(css, /\.venue-table :is\(th, td\)\.numeric\s*\{\s*padding-right:\s*56px/);
});

test('check-in action spacing is scoped and roster dates match their headers', () => {
    const view = read('Views/CheckIn/Index.cshtml');
    assert.equal((view.match(/table-sticky check-in-table/g) || []).length, 2);
    const css = read('Styles/features/check-in.css');
    assert.match(css, /\.check-in-table td\.actions\s*\{\s*text-align:\s*right/);
    assert.match(css, /\.check-in-table \.check-in-action-text\s*\{[^}]*padding-right:\s*8px/);
    const dossier = read('Views/Event/Details.cshtml');
    assert.match(dossier, /data-label="Submission date" class="numeric cell-date"/);
    assert.match(dossier, /data-label="Submission time" class="numeric cell-date"/);
    assert.match(dossier, /Model\.CancelledCount/);
    assert.match(read('Views/AccountRequest/Index.cshtml'), /data-label="Course \/ year \/ section"/);
});
