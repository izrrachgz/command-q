import { CommonModule } from '@angular/common';
import { DatePicker } from './components/date-picker/date-picker';
import { NgModule } from '@angular/core';

@NgModule({
  declarations: [DatePicker],
  exports: [DatePicker],
  imports: [CommonModule],
})
export class SharedModule {}
